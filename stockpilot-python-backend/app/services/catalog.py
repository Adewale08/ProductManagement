from datetime import datetime, timezone
from decimal import Decimal

from sqlalchemy import select
from sqlalchemy.exc import IntegrityError
from sqlalchemy.orm import Session, selectinload

from app.core.errors import APIError
from app.models import Business, Category, Product, Supplier
from app.schemas.catalog import CategoryQuery, CategoryUpdate, CategoryWrite, ProductUpdate, ProductWrite, StockUpdate, SupplierUpdate, SupplierWrite


def _product_response(product: Product) -> dict:
    return {
        "id": product.id,
        "name": product.name,
        "description": product.description,
        "sku": product.sku,
        "category": product.category.name if product.category is not None else product.category_name,
        "price": product.price,
        "stockQuantity": product.stock_quantity,
        "isActive": product.is_active,
        "createdAtUtc": product.created_at_utc,
        "updatedAtUtc": product.updated_at_utc,
        "categoryId": product.category_id,
        "supplierId": product.supplier_id,
    }


def _category_response(category: Category) -> dict:
    return {
        "id": category.id,
        "name": category.name,
        "description": category.description,
        "isActive": category.is_active,
        "createdAtUtc": category.created_at_utc,
        "updatedAtUtc": category.updated_at_utc,
    }


def _supplier_response(supplier: Supplier) -> dict:
    return {
        "id": supplier.id,
        "name": supplier.name,
        "email": supplier.email,
        "phoneNumber": supplier.phone_number,
        "isActive": supplier.is_active,
    }


def list_products(db: Session, business: Business) -> list[dict]:
    rows = db.scalars(
        select(Product)
        .options(selectinload(Product.category))
        .where(Product.business_id == business.id)
        .order_by(Product.name)
        .limit(100)
    ).all()
    return [_product_response(row) for row in rows]


def get_product(db: Session, business: Business, product_id: int) -> dict:
    product = db.scalar(
        select(Product)
        .options(selectinload(Product.category))
        .where(Product.id == product_id, Product.business_id == business.id)
    )
    if product is None:
        raise APIError(404, "This product was not found.")
    return _product_response(product)


def _category_name(db: Session, business_id: int, value: str, category_id: int | None) -> str:
    if category_id is None:
        return value
    category = db.scalar(select(Category).where(Category.id == category_id, Category.business_id == business_id))
    if category is None:
        raise APIError(400, "The selected category is unavailable for this business.")
    if len(category.name) > 80:
        raise APIError(400, "A category assigned to a product must have a name of at most 80 characters.")
    return category.name


def _validate_supplier(db: Session, business_id: int, supplier_id: int | None) -> None:
    if supplier_id is None:
        return
    exists = db.scalar(select(Supplier.id).where(Supplier.id == supplier_id, Supplier.business_id == business_id))
    if exists is None:
        raise APIError(400, "The selected supplier is unavailable for this business.")


def create_product(db: Session, business: Business, request: ProductWrite) -> dict:
    sku = request.sku.strip().upper()
    exists = db.scalar(select(Product.id).where(Product.sku == sku, Product.business_id == business.id).limit(1))
    if exists is not None:
        raise APIError(409, f"A product with SKU '{request.sku}' already exists.")
    category_name = _category_name(db, business.id, request.category, request.category_id)
    _validate_supplier(db, business.id, request.supplier_id)
    product = Product(
        name=request.name,
        description=request.description,
        sku=sku,
        category_name=category_name,
        category_id=request.category_id,
        supplier_id=request.supplier_id,
        price=request.price,
        stock_quantity=request.stock_quantity,
        is_active=True,
        created_at_utc=datetime.now(timezone.utc),
        business_id=business.id,
    )
    db.add(product)
    try:
        db.commit()
        db.refresh(product)
    except IntegrityError as exc:
        db.rollback()
        raise APIError(409, "A product with the specified SKU already exists.") from exc
    return _product_response(product)


def update_product(db: Session, business: Business, product_id: int, request: ProductUpdate) -> None:
    product = db.scalar(
        select(Product).options(selectinload(Product.category)).where(
            Product.id == product_id, Product.business_id == business.id
        )
    )
    if product is None:
        raise APIError(404, "This product was not found.")

    category_id = request.category_id
    existing_category_name = product.category.name if product.category is not None else product.category_name
    if category_id is None and product.category_id is not None and request.category.casefold() == existing_category_name.casefold():
        category_id = product.category_id
    supplier_id = request.supplier_id if request.supplier_id is not None else product.supplier_id
    category_name = _category_name(db, business.id, request.category, category_id)
    _validate_supplier(db, business.id, supplier_id)
    sku = request.sku.strip().upper()
    if sku.casefold() != product.sku.casefold():
        duplicate = db.scalar(select(Product.id).where(Product.sku == sku, Product.business_id == business.id, Product.id != product.id).limit(1))
        if duplicate is not None:
            raise APIError(409, f"A product with SKU '{request.sku}' already exists.")

    product.name = request.name
    product.description = request.description
    product.sku = sku
    product.category_name = category_name
    product.category_id = category_id
    product.supplier_id = supplier_id
    product.price = request.price
    product.stock_quantity = request.stock_quantity
    product.is_active = request.is_active
    product.updated_at_utc = datetime.now(timezone.utc)
    try:
        db.commit()
    except IntegrityError as exc:
        db.rollback()
        raise APIError(409, "A product with the specified SKU already exists.") from exc


def set_stock(db: Session, business: Business, product_id: int, request: StockUpdate) -> None:
    product = db.scalar(select(Product).where(Product.id == product_id, Product.business_id == business.id))
    if product is None:
        raise APIError(404, "This product was not found.")
    product.stock_quantity = request.stock_quantity
    product.updated_at_utc = datetime.now(timezone.utc)
    db.commit()


def delete_product(db: Session, business: Business, product_id: int) -> None:
    product = db.scalar(select(Product).where(Product.id == product_id, Product.business_id == business.id))
    if product is None:
        raise APIError(404, "This product was not found.")
    db.delete(product)
    db.commit()


def list_categories(db: Session, business: Business, query: CategoryQuery) -> list[dict]:
    statement = select(Category).where(Category.business_id == business.id)
    if query.name and query.name.strip():
        statement = statement.where(Category.name.contains(query.name.strip()))
    if query.is_active is not None:
        statement = statement.where(Category.is_active == query.is_active)
    rows = db.scalars(statement.offset((query.page - 1) * query.page_size).limit(query.page_size)).all()
    return [_category_response(row) for row in rows]


def get_category(db: Session, business: Business, category_id: int) -> dict:
    category = db.scalar(select(Category).where(Category.id == category_id, Category.business_id == business.id))
    if category is None:
        raise APIError(404, "Category not found.")
    return _category_response(category)


def create_category(db: Session, business: Business, request: CategoryWrite) -> dict:
    if not request.name.strip():
        raise APIError(400, "Name is required.")
    category = Category(
        name=request.name.strip(),
        description=request.description.strip() if request.description else None,
        is_active=True,
        created_at_utc=datetime.now(timezone.utc),
        business_id=business.id,
    )
    db.add(category)
    db.commit()
    db.refresh(category)
    return _category_response(category)


def update_category(db: Session, business: Business, category_id: int, request: CategoryUpdate) -> None:
    category = db.scalar(select(Category).where(Category.id == category_id, Category.business_id == business.id))
    if category is None:
        raise APIError(404, "Category not found.")
    if not request.name.strip():
        raise APIError(400, "Name is required.")
    category.name = request.name.strip()
    category.description = request.description.strip() if request.description else None
    category.is_active = request.is_active
    category.updated_at_utc = datetime.now(timezone.utc)
    db.commit()


def delete_category(db: Session, business: Business, category_id: int) -> None:
    category = db.scalar(select(Category).where(Category.id == category_id, Category.business_id == business.id))
    if category is None:
        raise APIError(404, "Category not found.")
    used = db.scalar(select(Product.id).where(Product.category_id == category.id, Product.business_id == business.id).limit(1))
    if used is not None:
        raise APIError(409, "Remove product references before deleting this category.")
    db.delete(category)
    db.commit()


def list_suppliers(db: Session, business: Business) -> list[dict]:
    rows = db.scalars(select(Supplier).where(Supplier.business_id == business.id).order_by(Supplier.name)).all()
    return [_supplier_response(row) for row in rows]


def get_supplier(db: Session, business: Business, supplier_id: int) -> dict:
    supplier = db.scalar(select(Supplier).where(Supplier.id == supplier_id, Supplier.business_id == business.id))
    if supplier is None:
        raise APIError(404, "Supplier not found.")
    return _supplier_response(supplier)


def _check_supplier_email(db: Session, business_id: int, email: str, except_id: int | None = None) -> None:
    statement = select(Supplier.id).where(Supplier.business_id == business_id, Supplier.email == email)
    if except_id is not None:
        statement = statement.where(Supplier.id != except_id)
    if db.scalar(statement.limit(1)) is not None:
        raise APIError(409, "A supplier with this email already exists in this business.")


def create_supplier(db: Session, business: Business, request: SupplierWrite) -> dict:
    email = str(request.email).strip().lower()
    _check_supplier_email(db, business.id, email)
    supplier = Supplier(
        name=request.name.strip(),
        email=email,
        phone_number=request.phone_number.strip() if request.phone_number else None,
        is_active=True,
        business_id=business.id,
    )
    db.add(supplier)
    try:
        db.commit()
        db.refresh(supplier)
    except IntegrityError as exc:
        db.rollback()
        raise APIError(409, "A supplier with this email already exists in this business.") from exc
    return _supplier_response(supplier)


def update_supplier(db: Session, business: Business, supplier_id: int, request: SupplierUpdate) -> None:
    supplier = db.scalar(select(Supplier).where(Supplier.id == supplier_id, Supplier.business_id == business.id))
    if supplier is None:
        raise APIError(404, "Supplier not found.")
    email = str(request.email).strip().lower()
    _check_supplier_email(db, business.id, email, supplier.id)
    supplier.name = request.name.strip()
    supplier.email = email
    supplier.phone_number = request.phone_number.strip() if request.phone_number else None
    supplier.is_active = request.is_active
    try:
        db.commit()
    except IntegrityError as exc:
        db.rollback()
        raise APIError(409, "A supplier with this email already exists in this business.") from exc


def delete_supplier(db: Session, business: Business, supplier_id: int) -> None:
    supplier = db.scalar(select(Supplier).where(Supplier.id == supplier_id, Supplier.business_id == business.id))
    if supplier is None:
        raise APIError(404, "Supplier not found.")
    active_product = db.scalar(select(Product.id).where(Product.supplier_id == supplier.id, Product.business_id == business.id, Product.is_active.is_(True)).limit(1))
    if active_product is not None:
        raise APIError(409, "Supplier has active products.")
    any_product = db.scalar(select(Product.id).where(Product.supplier_id == supplier.id, Product.business_id == business.id).limit(1))
    if any_product is not None:
        raise APIError(409, "Remove product references before deleting supplier.")
    db.delete(supplier)
    db.commit()