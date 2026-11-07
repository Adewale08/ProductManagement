import os

os.environ.setdefault("DATABASE_URL", "sqlite+pysqlite:///:memory:")
os.environ.setdefault("SESSION_SECRET", "test-session-secret-that-is-at-least-32-characters")
os.environ.setdefault("ENVIRONMENT", "test")

import pytest
from fastapi.testclient import TestClient
from sqlalchemy import create_engine, event
from sqlalchemy.orm import sessionmaker
from sqlalchemy.pool import StaticPool

from app.database import Base, get_db
from app.main import app


@pytest.fixture()
def client():
    engine = create_engine(
        "sqlite+pysqlite:///:memory:",
        connect_args={"check_same_thread": False},
        poolclass=StaticPool,
    )

    @event.listens_for(engine, "connect")
    def enable_foreign_keys(connection, _record):
        cursor = connection.cursor()
        cursor.execute("PRAGMA foreign_keys=ON")
        cursor.close()

    Base.metadata.create_all(engine)
    session_factory = sessionmaker(bind=engine, autoflush=False, expire_on_commit=False)

    def override_get_db():
        with session_factory() as session:
            yield session

    app.dependency_overrides[get_db] = override_get_db
    with TestClient(app) as test_client:
        yield test_client
    app.dependency_overrides.clear()
    Base.metadata.drop_all(engine)
    engine.dispose()


def register_and_login(client: TestClient, email: str, business_name: str) -> None:
    response = client.post(
        "/api/Auth/register",
        json={
            "email": email,
            "password": "ValidPass1!",
            "businessName": business_name,
            "fullName": "StockPilot Owner",
        },
    )
    assert response.status_code == 201, response.text
    response = client.post("/api/Auth/login", json={"email": email, "password": "ValidPass1!"})
    assert response.status_code == 200, response.text
    assert "stockpilot_session" in response.cookies


def product_payload(**overrides):
    payload = {
        "name": "Oats",
        "description": None,
        "sku": "oat-001",
        "category": "Pantry",
        "price": 4.25,
        "stockQuantity": 8,
    }
    payload.update(overrides)
    return payload


def test_auth_contract_and_logout(client: TestClient):
    register_and_login(client, "owner@example.com", "Good Pantry")
    me = client.get("/api/Auth/me")
    assert me.status_code == 200
    assert me.json() == {
        "userId": me.json()["userId"],
        "businessName": "Good Pantry",
        "email": "owner@example.com",
    }
    logout = client.post("/api/Auth/logout")
    assert logout.status_code == 200
    assert client.get("/api/Auth/me").status_code == 401


def test_catalog_is_scoped_and_ignores_client_business_id(client: TestClient):
    register_and_login(client, "one@example.com", "Business One")
    category = client.post("/Category", json={"name": "Pantry"})
    supplier = client.post("/api/Suppliers", json={"name": "Farm Co", "email": "orders@farm.example"})
    created = client.post(
        "/Products",
        json=product_payload(
            businessId=9999,
            categoryId=category.json()["id"],
            supplierId=supplier.json()["id"],
        ),
    )
    assert created.status_code == 201, created.text
    product = created.json()
    assert product["sku"] == "OAT-001"
    assert isinstance(product["price"], (int, float))
    assert "businessId" not in product
    order = client.post("/Orders", json={"items": [{"productId": product["id"], "quantity": 1}]})
    assert order.status_code == 201, order.text

    with TestClient(app) as other:
        register_and_login(other, "two@example.com", "Business Two")
        assert other.get("/Products").json() == []
        assert other.get(f"/Products/{product['id']}").status_code == 404
        assert other.delete(f"/Products/{product['id']}").status_code == 404
        assert other.get(f"/Category/{category.json()['id']}").status_code == 404
        assert other.get(f"/api/Suppliers/{supplier.json()['id']}").status_code == 404
        assert other.get(f"/Orders/{order.json()['id']}").status_code == 404
        foreign_reference = other.post(
            "/Products",
            json=product_payload(categoryId=category.json()["id"], supplierId=supplier.json()["id"]),
        )
        assert foreign_reference.status_code == 400
        foreign_order = other.post("/Orders", json={"items": [{"productId": product["id"], "quantity": 1}]})
        assert foreign_order.status_code == 400

def test_category_supplier_and_reference_guards(client: TestClient):
    register_and_login(client, "catalog@example.com", "Catalog Business")
    category = client.post("/Category", json={"name": "Pantry", "description": None})
    assert category.status_code == 201, category.text
    supplier = client.post(
        "/api/Suppliers",
        json={"name": "Farm Co", "email": "ORDERS@FARM.EXAMPLE", "phoneNumber": "123"},
    )
    assert supplier.status_code == 201, supplier.text
    assert supplier.json()["email"] == "orders@farm.example"

    product = client.post(
        "/Products",
        json=product_payload(categoryId=category.json()["id"], supplierId=supplier.json()["id"]),
    )
    assert product.status_code == 201, product.text
    assert product.json()["category"] == "Pantry"
    assert client.delete(f"/Category/{category.json()['id']}").status_code == 409
    assert client.delete(f"/api/Suppliers/{supplier.json()['id']}").status_code == 409


def test_order_decrements_stock_and_cancellation_restores_it(client: TestClient):
    register_and_login(client, "sales@example.com", "Sales Business")
    product = client.post("/Products", json=product_payload())
    assert product.status_code == 201, product.text
    product_id = product.json()["id"]

    order = client.post(
        "/Orders",
        json={"items": [{"productId": product_id, "quantity": 2}, {"productId": product_id, "quantity": 1}]},
    )
    assert order.status_code == 201, order.text
    assert order.json()["totalAmount"] == 12.75
    assert order.json()["items"][0]["quantity"] == 3
    assert client.get(f"/Products/{product_id}").json()["stockQuantity"] == 5

    cancelled = client.patch(f"/Orders/{order.json()['id']}/status", json={"status": "cancelled"})
    assert cancelled.status_code == 200, cancelled.text
    assert cancelled.json()["status"] == "Cancelled"
    assert client.get(f"/Products/{product_id}").json()["stockQuantity"] == 8
    assert client.patch(f"/Orders/{order.json()['id']}/status", json={"status": "Completed"}).status_code == 409

def test_catalog_updates_and_stock_patch(client: TestClient):
    register_and_login(client, "updates@example.com", "Update Business")
    category = client.post("/Category", json={"name": "Pantry", "description": "Dry goods"})
    supplier = client.post("/api/Suppliers", json={"name": "Farm Co", "email": "orders@farm.example"})
    product = client.post(
        "/Products",
        json=product_payload(categoryId=category.json()["id"], supplierId=supplier.json()["id"]),
    )
    product_id = product.json()["id"]

    updated = client.put(
        f"/Products/{product_id}",
        json=product_payload(
            name="Rolled oats",
            sku="oat-002",
            category="Pantry",
            categoryId=category.json()["id"],
            supplierId=supplier.json()["id"],
            price=5.5,
            stockQuantity=10,
            isActive=False,
        ),
    )
    assert updated.status_code == 204
    assert client.get(f"/Products/{product_id}").json()["name"] == "Rolled oats"
    assert client.patch(f"/Products/{product_id}/stock", json={"stockQuantity": 12}).status_code == 204
    assert client.get(f"/Products/{product_id}").json()["stockQuantity"] == 12

    assert client.put(
        f"/Category/{category.json()['id']}",
        json={"name": "Staples", "description": None, "isActive": False},
    ).status_code == 204
    assert client.put(
        f"/api/Suppliers/{supplier.json()['id']}",
        json={"name": "Farm Supply", "email": "orders@farm.example", "phoneNumber": None, "isActive": False},
    ).status_code == 204

def test_state_changing_requests_reject_untrusted_origins(client: TestClient):
    register_and_login(client, "csrf@example.com", "CSRF Business")
    response = client.post("/Category", headers={"Origin": "https://hostile.example"}, json={"name": "Nope"})
    assert response.status_code == 403
    assert client.get("/Category").status_code == 200