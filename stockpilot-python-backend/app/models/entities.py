from __future__ import annotations

from datetime import datetime, timezone
from decimal import Decimal

from sqlalchemy import Boolean, DateTime, ForeignKey, ForeignKeyConstraint, Integer, Numeric, String, Text, UniqueConstraint
from sqlalchemy.orm import Mapped, mapped_column, relationship

from app.database import Base


class ApplicationUser(Base):
    __tablename__ = "AspNetUsers"

    id: Mapped[str] = mapped_column("Id", Text, primary_key=True)
    user_name: Mapped[str | None] = mapped_column("UserName", String(256))
    normalized_user_name: Mapped[str | None] = mapped_column("NormalizedUserName", String(256), unique=True)
    email: Mapped[str | None] = mapped_column("Email", String(256))
    normalized_email: Mapped[str | None] = mapped_column("NormalizedEmail", String(256), index=True)
    email_confirmed: Mapped[bool] = mapped_column("EmailConfirmed", Boolean, default=False, nullable=False)
    password_hash: Mapped[str | None] = mapped_column("PasswordHash", Text)
    security_stamp: Mapped[str | None] = mapped_column("SecurityStamp", Text)
    concurrency_stamp: Mapped[str | None] = mapped_column("ConcurrencyStamp", Text)
    phone_number: Mapped[str | None] = mapped_column("PhoneNumber", Text)
    phone_number_confirmed: Mapped[bool] = mapped_column("PhoneNumberConfirmed", Boolean, default=False, nullable=False)
    two_factor_enabled: Mapped[bool] = mapped_column("TwoFactorEnabled", Boolean, default=False, nullable=False)
    lockout_end: Mapped[datetime | None] = mapped_column("LockoutEnd", DateTime(timezone=True))
    lockout_enabled: Mapped[bool] = mapped_column("LockoutEnabled", Boolean, default=False, nullable=False)
    access_failed_count: Mapped[int] = mapped_column("AccessFailedCount", Integer, default=0, nullable=False)

    businesses: Mapped[list[Business]] = relationship(back_populates="owner")
    claims: Mapped[list[ApplicationUserClaim]] = relationship(back_populates="user", cascade="all, delete-orphan")
    logins: Mapped[list[ApplicationUserLogin]] = relationship(back_populates="user", cascade="all, delete-orphan")
    roles: Mapped[list[ApplicationUserRole]] = relationship(back_populates="user", cascade="all, delete-orphan")
    tokens: Mapped[list[ApplicationUserToken]] = relationship(back_populates="user", cascade="all, delete-orphan")


class ApplicationRole(Base):
    __tablename__ = "AspNetRoles"

    id: Mapped[str] = mapped_column("Id", Text, primary_key=True)
    name: Mapped[str | None] = mapped_column("Name", String(256))
    normalized_name: Mapped[str | None] = mapped_column("NormalizedName", String(256), unique=True)
    concurrency_stamp: Mapped[str | None] = mapped_column("ConcurrencyStamp", Text)
    claims: Mapped[list[ApplicationRoleClaim]] = relationship(back_populates="role", cascade="all, delete-orphan")
    users: Mapped[list[ApplicationUserRole]] = relationship(back_populates="role", cascade="all, delete-orphan")


class ApplicationUserClaim(Base):
    __tablename__ = "AspNetUserClaims"

    id: Mapped[int] = mapped_column("Id", Integer, primary_key=True, autoincrement=True)
    user_id: Mapped[str] = mapped_column("UserId", ForeignKey("AspNetUsers.Id", ondelete="CASCADE"), nullable=False, index=True)
    claim_type: Mapped[str | None] = mapped_column("ClaimType", Text)
    claim_value: Mapped[str | None] = mapped_column("ClaimValue", Text)
    user: Mapped[ApplicationUser] = relationship(back_populates="claims")


class ApplicationRoleClaim(Base):
    __tablename__ = "AspNetRoleClaims"

    id: Mapped[int] = mapped_column("Id", Integer, primary_key=True, autoincrement=True)
    role_id: Mapped[str] = mapped_column("RoleId", ForeignKey("AspNetRoles.Id", ondelete="CASCADE"), nullable=False, index=True)
    claim_type: Mapped[str | None] = mapped_column("ClaimType", Text)
    claim_value: Mapped[str | None] = mapped_column("ClaimValue", Text)
    role: Mapped[ApplicationRole] = relationship(back_populates="claims")


class ApplicationUserLogin(Base):
    __tablename__ = "AspNetUserLogins"

    login_provider: Mapped[str] = mapped_column("LoginProvider", Text, primary_key=True)
    provider_key: Mapped[str] = mapped_column("ProviderKey", Text, primary_key=True)
    provider_display_name: Mapped[str | None] = mapped_column("ProviderDisplayName", Text)
    user_id: Mapped[str] = mapped_column("UserId", ForeignKey("AspNetUsers.Id", ondelete="CASCADE"), nullable=False, index=True)
    user: Mapped[ApplicationUser] = relationship(back_populates="logins")


class ApplicationUserRole(Base):
    __tablename__ = "AspNetUserRoles"

    user_id: Mapped[str] = mapped_column("UserId", ForeignKey("AspNetUsers.Id", ondelete="CASCADE"), primary_key=True)
    role_id: Mapped[str] = mapped_column("RoleId", ForeignKey("AspNetRoles.Id", ondelete="CASCADE"), primary_key=True, index=True)
    user: Mapped[ApplicationUser] = relationship(back_populates="roles")
    role: Mapped[ApplicationRole] = relationship(back_populates="users")


class ApplicationUserToken(Base):
    __tablename__ = "AspNetUserTokens"

    user_id: Mapped[str] = mapped_column("UserId", ForeignKey("AspNetUsers.Id", ondelete="CASCADE"), primary_key=True)
    login_provider: Mapped[str] = mapped_column("LoginProvider", Text, primary_key=True)
    name: Mapped[str] = mapped_column("Name", Text, primary_key=True)
    value: Mapped[str | None] = mapped_column("Value", Text)
    user: Mapped[ApplicationUser] = relationship(back_populates="tokens")


class Business(Base):
    __tablename__ = "Business"

    id: Mapped[int] = mapped_column("Id", Integer, primary_key=True, autoincrement=True)
    name: Mapped[str] = mapped_column("Name", Text, nullable=False)
    created_at_utc: Mapped[datetime] = mapped_column("CreatedAtUtc", DateTime(timezone=True), default=lambda: datetime.now(timezone.utc), nullable=False)
    owner_id: Mapped[str] = mapped_column("OwnerId", ForeignKey("AspNetUsers.Id", ondelete="CASCADE"), nullable=False, index=True)
    owner: Mapped[ApplicationUser] = relationship(back_populates="businesses")
    products: Mapped[list[Product]] = relationship(back_populates="business")
    suppliers: Mapped[list[Supplier]] = relationship(back_populates="business")
    categories: Mapped[list[Category]] = relationship(back_populates="business")
    orders: Mapped[list[Order]] = relationship(back_populates="business")


class Category(Base):
    __tablename__ = "Categories"
    __table_args__ = (UniqueConstraint("Id", "BusinessId", name="uq_categories_id_business_id"),)

    id: Mapped[int] = mapped_column("Id", Integer, primary_key=True, autoincrement=True)
    name: Mapped[str] = mapped_column("Name", String(100), nullable=False)
    description: Mapped[str | None] = mapped_column("Description", String(250))
    created_at_utc: Mapped[datetime] = mapped_column("CreatedAtUtc", DateTime(timezone=True), default=lambda: datetime.now(timezone.utc), nullable=False)
    updated_at_utc: Mapped[datetime | None] = mapped_column("UpdatedAtUtc", DateTime(timezone=True))
    is_active: Mapped[bool] = mapped_column("IsActive", Boolean, default=True, nullable=False)
    business_id: Mapped[int | None] = mapped_column("BusinessId", ForeignKey("Business.Id", ondelete="RESTRICT"), index=True)
    business: Mapped[Business | None] = relationship(back_populates="categories")
    products: Mapped[list[Product]] = relationship(back_populates="category")


class Supplier(Base):
    __tablename__ = "Suppliers"
    __table_args__ = (
        UniqueConstraint("BusinessId", "Email", name="uq_suppliers_business_id_email"),
        UniqueConstraint("Id", "BusinessId", name="uq_suppliers_id_business_id"),
    )

    id: Mapped[int] = mapped_column("Id", Integer, primary_key=True, autoincrement=True)
    name: Mapped[str] = mapped_column("Name", String(120), nullable=False)
    email: Mapped[str] = mapped_column("Email", String(254), nullable=False)
    phone_number: Mapped[str | None] = mapped_column("PhoneNumber", String(30))
    is_active: Mapped[bool] = mapped_column("IsActive", Boolean, default=True, nullable=False)
    business_id: Mapped[int | None] = mapped_column("BusinessId", ForeignKey("Business.Id", ondelete="RESTRICT"), index=True)
    business: Mapped[Business | None] = relationship(back_populates="suppliers")
    products: Mapped[list[Product]] = relationship(back_populates="supplier")


class Product(Base):
    __tablename__ = "Products"
    __table_args__ = (
        UniqueConstraint("Sku", name="uq_products_sku"),
        ForeignKeyConstraint(["CategoryId"], ["Categories.Id"], name="fk_products_category", ondelete="RESTRICT"),
        ForeignKeyConstraint(["SupplierId"], ["Suppliers.Id"], name="fk_products_supplier", ondelete="RESTRICT"),
    )

    id: Mapped[int] = mapped_column("Id", Integer, primary_key=True, autoincrement=True)
    name: Mapped[str] = mapped_column("Name", String(120), nullable=False)
    description: Mapped[str | None] = mapped_column("Description", String(500))
    sku: Mapped[str] = mapped_column("Sku", String(40), nullable=False)
    category_name: Mapped[str] = mapped_column("Category", String(80), nullable=False)
    price: Mapped[Decimal] = mapped_column("Price", Numeric(18, 2), nullable=False)
    stock_quantity: Mapped[int] = mapped_column("StockQuantity", Integer, nullable=False)
    is_active: Mapped[bool] = mapped_column("IsActive", Boolean, default=True, nullable=False)
    created_at_utc: Mapped[datetime] = mapped_column("CreatedAtUtc", DateTime(timezone=True), default=lambda: datetime.now(timezone.utc), nullable=False)
    updated_at_utc: Mapped[datetime | None] = mapped_column("UpdatedAtUtc", DateTime(timezone=True))
    supplier_id: Mapped[int | None] = mapped_column("SupplierId", Integer, index=True)
    category_id: Mapped[int | None] = mapped_column("CategoryId", Integer, index=True)
    business_id: Mapped[int | None] = mapped_column("BusinessId", ForeignKey("Business.Id"), index=True)
    business: Mapped[Business | None] = relationship(back_populates="products")
    supplier: Mapped[Supplier | None] = relationship(back_populates="products")
    category: Mapped[Category | None] = relationship(back_populates="products")


class Order(Base):
    __tablename__ = "Orders"

    id: Mapped[int] = mapped_column("Id", Integer, primary_key=True, autoincrement=True)
    business_id: Mapped[int] = mapped_column("BusinessId", ForeignKey("Business.Id", ondelete="RESTRICT"), nullable=False, index=True)
    created_at_utc: Mapped[datetime] = mapped_column("CreatedAtUtc", DateTime(timezone=True), default=lambda: datetime.now(timezone.utc), nullable=False)
    status: Mapped[str] = mapped_column("Status", Text, nullable=False, default="Pending")
    total_amount: Mapped[Decimal] = mapped_column("TotalAmount", Numeric(18, 2), nullable=False)
    business: Mapped[Business] = relationship(back_populates="orders")
    items: Mapped[list[OrderItem]] = relationship(back_populates="order")


class OrderItem(Base):
    __tablename__ = "OrderItems"

    id: Mapped[int] = mapped_column("Id", Integer, primary_key=True, autoincrement=True)
    order_id: Mapped[int] = mapped_column("OrderId", ForeignKey("Orders.Id", ondelete="RESTRICT"), nullable=False, index=True)
    product_id: Mapped[int] = mapped_column("ProductId", ForeignKey("Products.Id", ondelete="RESTRICT"), nullable=False, index=True)
    quantity: Mapped[int] = mapped_column("Quantity", Integer, nullable=False)
    unit_price: Mapped[Decimal] = mapped_column("UnitPrice", Numeric(18, 2), nullable=False)
    order: Mapped[Order] = relationship(back_populates="items")
    product: Mapped[Product] = relationship()