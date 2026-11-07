from collections import defaultdict
from datetime import datetime, timezone
from decimal import Decimal

from sqlalchemy import select
from sqlalchemy.orm import Session, selectinload

from app.core.errors import APIError
from app.models import Business, Order, OrderItem, Product
from app.schemas.orders import CreateOrder, UpdateOrderStatus

_MAX_SQL_AMOUNT = Decimal("9999999999999999.99")


def _response(order: Order, product_names: dict[int, str] | None = None) -> dict:
    items = []
    for item in order.items:
        if product_names is not None:
            name = product_names[item.product_id]
        else:
            name = item.product.name
        items.append({
            "productId": item.product_id,
            "productName": name,
            "quantity": item.quantity,
            "unitPrice": item.unit_price,
            "lineTotal": item.unit_price * item.quantity,
        })
    return {
        "id": order.id,
        "createdAtUtc": order.created_at_utc,
        "status": order.status,
        "totalAmount": order.total_amount,
        "items": items,
    }


def create_order(db: Session, business: Business, request: CreateOrder) -> dict:
    quantities: dict[int, int] = defaultdict(int)
    for item in request.items:
        combined = quantities[item.product_id] + item.quantity
        if combined > 2_147_483_647:
            raise APIError(400, "The combined quantity for a product is too large.")
        quantities[item.product_id] = combined

    product_ids = sorted(quantities)
    try:
        products = db.scalars(
            select(Product)
            .where(Product.id.in_(product_ids), Product.business_id == business.id)
            .order_by(Product.id)
            .with_for_update()
        ).all()
        by_id = {product.id: product for product in products}
        if len(by_id) != len(product_ids):
            raise APIError(400, "One or more products are unavailable for this business.")

        for product_id in product_ids:
            product = by_id[product_id]
            quantity = quantities[product_id]
            if not product.is_active:
                raise APIError(409, f"Product '{product.name}' is not active.")
            if product.price < 0:
                raise APIError(400, f"Product '{product.name}' has an invalid price.")
            if product.stock_quantity < quantity:
                raise APIError(409, f"Insufficient stock for product '{product.name}'.")

        total = Decimal("0.00")
        order_items: list[OrderItem] = []
        for product_id in product_ids:
            product = by_id[product_id]
            quantity = quantities[product_id]
            line_total = product.price * quantity
            if line_total > _MAX_SQL_AMOUNT or total > _MAX_SQL_AMOUNT - line_total:
                raise APIError(400, "The order total exceeds the supported amount.")
            total += line_total
            order_items.append(OrderItem(product=product, quantity=quantity, unit_price=product.price))

        now = datetime.now(timezone.utc)
        for product_id in product_ids:
            product = by_id[product_id]
            product.stock_quantity -= quantities[product_id]
            product.updated_at_utc = now

        order = Order(
            business_id=business.id,
            created_at_utc=now,
            status="Pending",
            total_amount=total,
            items=order_items,
        )
        db.add(order)
        db.commit()
        db.refresh(order)
        return _response(order, {product_id: by_id[product_id].name for product_id in product_ids})
    except Exception:
        db.rollback()
        raise


def list_orders(db: Session, business: Business) -> list[dict]:
    orders = db.scalars(
        select(Order)
        .options(selectinload(Order.items).selectinload(OrderItem.product))
        .where(Order.business_id == business.id)
        .order_by(Order.created_at_utc.desc())
    ).all()
    return [_response(order) for order in orders]


def get_order(db: Session, business: Business, order_id: int) -> dict:
    order = db.scalar(
        select(Order)
        .options(selectinload(Order.items).selectinload(OrderItem.product))
        .where(Order.id == order_id, Order.business_id == business.id)
    )
    if order is None:
        raise APIError(404, f"Order {order_id} was not found.")
    return _response(order)


def update_order_status(db: Session, business: Business, order_id: int, request: UpdateOrderStatus) -> dict:
    normalized = request.status.strip().lower()
    if normalized == "completed":
        new_status = "Completed"
    elif normalized == "cancelled":
        new_status = "Cancelled"
    else:
        raise APIError(400, "Status must be either 'Completed' or 'Cancelled'.")

    try:
        order = db.scalar(
            select(Order)
            .options(selectinload(Order.items).selectinload(OrderItem.product))
            .where(Order.id == order_id, Order.business_id == business.id)
            .with_for_update()
        )
        if order is None:
            raise APIError(404, f"Order {order_id} was not found.")
        if order.status != "Pending":
            raise APIError(409, "Only pending orders can be completed or cancelled.")

        if new_status == "Cancelled":
            now = datetime.now(timezone.utc)
            for item in order.items:
                product = db.scalar(
                    select(Product)
                    .where(Product.id == item.product_id, Product.business_id == business.id)
                    .with_for_update()
                )
                if product is None:
                    raise APIError(409, "Inventory could not be restored for the cancelled order.")
                product.stock_quantity += item.quantity
                product.updated_at_utc = now

        order.status = new_status
        db.commit()
        db.refresh(order)
        order = db.scalar(
            select(Order)
            .options(selectinload(Order.items).selectinload(OrderItem.product))
            .where(Order.id == order_id, Order.business_id == business.id)
        )
        if order is None:
            raise APIError(409, "The order could not be loaded after its status changed.")
        return _response(order)
    except Exception:
        db.rollback()
        raise