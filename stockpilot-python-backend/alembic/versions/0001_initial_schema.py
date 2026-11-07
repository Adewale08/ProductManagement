"""Create StockPilot and Identity tables for the fresh Supabase database.

Revision ID: 0001_initial_schema
Revises:
"""
from typing import Sequence, Union

from alembic import op
import sqlalchemy as sa

revision: str = "0001_initial_schema"
down_revision: Union[str, None] = None
branch_labels: Union[str, Sequence[str], None] = None
depends_on: Union[str, Sequence[str], None] = None


def upgrade() -> None:
    op.create_table(
        "AspNetRoles",
        sa.Column("Id", sa.Text(), nullable=False),
        sa.Column("Name", sa.String(256), nullable=True),
        sa.Column("NormalizedName", sa.String(256), nullable=True),
        sa.Column("ConcurrencyStamp", sa.Text(), nullable=True),
        sa.PrimaryKeyConstraint("Id", name="pk_AspNetRoles"),
        sa.UniqueConstraint("NormalizedName", name="uq_AspNetRoles_NormalizedName"),
    )
    op.create_table(
        "AspNetUsers",
        sa.Column("Id", sa.Text(), nullable=False),
        sa.Column("UserName", sa.String(256), nullable=True),
        sa.Column("NormalizedUserName", sa.String(256), nullable=True),
        sa.Column("Email", sa.String(256), nullable=True),
        sa.Column("NormalizedEmail", sa.String(256), nullable=True),
        sa.Column("EmailConfirmed", sa.Boolean(), nullable=False),
        sa.Column("PasswordHash", sa.Text(), nullable=True),
        sa.Column("SecurityStamp", sa.Text(), nullable=True),
        sa.Column("ConcurrencyStamp", sa.Text(), nullable=True),
        sa.Column("PhoneNumber", sa.Text(), nullable=True),
        sa.Column("PhoneNumberConfirmed", sa.Boolean(), nullable=False),
        sa.Column("TwoFactorEnabled", sa.Boolean(), nullable=False),
        sa.Column("LockoutEnd", sa.DateTime(timezone=True), nullable=True),
        sa.Column("LockoutEnabled", sa.Boolean(), nullable=False),
        sa.Column("AccessFailedCount", sa.Integer(), nullable=False),
        sa.PrimaryKeyConstraint("Id", name="pk_AspNetUsers"),
        sa.UniqueConstraint("NormalizedUserName", name="uq_AspNetUsers_NormalizedUserName"),
    )
    op.create_index("ix_AspNetUsers_NormalizedEmail", "AspNetUsers", ["NormalizedEmail"])
    op.create_table(
        "AspNetRoleClaims",
        sa.Column("Id", sa.Integer(), sa.Identity(), nullable=False),
        sa.Column("RoleId", sa.Text(), nullable=False),
        sa.Column("ClaimType", sa.Text(), nullable=True),
        sa.Column("ClaimValue", sa.Text(), nullable=True),
        sa.ForeignKeyConstraint(["RoleId"], ["AspNetRoles.Id"], name="fk_AspNetRoleClaims_RoleId_AspNetRoles", ondelete="CASCADE"),
        sa.PrimaryKeyConstraint("Id", name="pk_AspNetRoleClaims"),
    )
    op.create_index("ix_AspNetRoleClaims_RoleId", "AspNetRoleClaims", ["RoleId"])
    op.create_table(
        "AspNetUserClaims",
        sa.Column("Id", sa.Integer(), sa.Identity(), nullable=False),
        sa.Column("UserId", sa.Text(), nullable=False),
        sa.Column("ClaimType", sa.Text(), nullable=True),
        sa.Column("ClaimValue", sa.Text(), nullable=True),
        sa.ForeignKeyConstraint(["UserId"], ["AspNetUsers.Id"], name="fk_AspNetUserClaims_UserId_AspNetUsers", ondelete="CASCADE"),
        sa.PrimaryKeyConstraint("Id", name="pk_AspNetUserClaims"),
    )
    op.create_index("ix_AspNetUserClaims_UserId", "AspNetUserClaims", ["UserId"])
    op.create_table(
        "AspNetUserLogins",
        sa.Column("LoginProvider", sa.Text(), nullable=False),
        sa.Column("ProviderKey", sa.Text(), nullable=False),
        sa.Column("ProviderDisplayName", sa.Text(), nullable=True),
        sa.Column("UserId", sa.Text(), nullable=False),
        sa.ForeignKeyConstraint(["UserId"], ["AspNetUsers.Id"], name="fk_AspNetUserLogins_UserId_AspNetUsers", ondelete="CASCADE"),
        sa.PrimaryKeyConstraint("LoginProvider", "ProviderKey", name="pk_AspNetUserLogins"),
    )
    op.create_index("ix_AspNetUserLogins_UserId", "AspNetUserLogins", ["UserId"])
    op.create_table(
        "AspNetUserRoles",
        sa.Column("UserId", sa.Text(), nullable=False),
        sa.Column("RoleId", sa.Text(), nullable=False),
        sa.ForeignKeyConstraint(["RoleId"], ["AspNetRoles.Id"], name="fk_AspNetUserRoles_RoleId_AspNetRoles", ondelete="CASCADE"),
        sa.ForeignKeyConstraint(["UserId"], ["AspNetUsers.Id"], name="fk_AspNetUserRoles_UserId_AspNetUsers", ondelete="CASCADE"),
        sa.PrimaryKeyConstraint("UserId", "RoleId", name="pk_AspNetUserRoles"),
    )
    op.create_index("ix_AspNetUserRoles_RoleId", "AspNetUserRoles", ["RoleId"])
    op.create_table(
        "AspNetUserTokens",
        sa.Column("UserId", sa.Text(), nullable=False),
        sa.Column("LoginProvider", sa.Text(), nullable=False),
        sa.Column("Name", sa.Text(), nullable=False),
        sa.Column("Value", sa.Text(), nullable=True),
        sa.ForeignKeyConstraint(["UserId"], ["AspNetUsers.Id"], name="fk_AspNetUserTokens_UserId_AspNetUsers", ondelete="CASCADE"),
        sa.PrimaryKeyConstraint("UserId", "LoginProvider", "Name", name="pk_AspNetUserTokens"),
    )
    op.create_table(
        "Business",
        sa.Column("Id", sa.Integer(), sa.Identity(), nullable=False),
        sa.Column("Name", sa.Text(), nullable=False),
        sa.Column("CreatedAtUtc", sa.DateTime(timezone=True), nullable=False),
        sa.Column("OwnerId", sa.Text(), nullable=False),
        sa.ForeignKeyConstraint(["OwnerId"], ["AspNetUsers.Id"], name="fk_Business_OwnerId_AspNetUsers", ondelete="CASCADE"),
        sa.PrimaryKeyConstraint("Id", name="pk_Business"),
    )
    op.create_index("ix_Business_OwnerId", "Business", ["OwnerId"])
    op.create_table(
        "Categories",
        sa.Column("Id", sa.Integer(), sa.Identity(), nullable=False),
        sa.Column("Name", sa.String(100), nullable=False),
        sa.Column("Description", sa.String(250), nullable=True),
        sa.Column("CreatedAtUtc", sa.DateTime(timezone=True), nullable=False),
        sa.Column("UpdatedAtUtc", sa.DateTime(timezone=True), nullable=True),
        sa.Column("IsActive", sa.Boolean(), nullable=False),
        sa.Column("BusinessId", sa.Integer(), nullable=True),
        sa.ForeignKeyConstraint(["BusinessId"], ["Business.Id"], name="fk_Categories_BusinessId_Business", ondelete="RESTRICT"),
        sa.PrimaryKeyConstraint("Id", name="pk_Categories"),
        sa.UniqueConstraint("Id", "BusinessId", name="uq_categories_id_business_id"),
    )
    op.create_index("ix_Categories_BusinessId", "Categories", ["BusinessId"])
    op.create_table(
        "Orders",
        sa.Column("Id", sa.Integer(), sa.Identity(), nullable=False),
        sa.Column("BusinessId", sa.Integer(), nullable=False),
        sa.Column("CreatedAtUtc", sa.DateTime(timezone=True), nullable=False),
        sa.Column("Status", sa.Text(), nullable=False),
        sa.Column("TotalAmount", sa.Numeric(18, 2), nullable=False),
        sa.ForeignKeyConstraint(["BusinessId"], ["Business.Id"], name="fk_Orders_BusinessId_Business", ondelete="RESTRICT"),
        sa.PrimaryKeyConstraint("Id", name="pk_Orders"),
    )
    op.create_index("ix_Orders_BusinessId", "Orders", ["BusinessId"])
    op.create_table(
        "Suppliers",
        sa.Column("Id", sa.Integer(), sa.Identity(), nullable=False),
        sa.Column("Name", sa.String(120), nullable=False),
        sa.Column("Email", sa.String(254), nullable=False),
        sa.Column("PhoneNumber", sa.String(30), nullable=True),
        sa.Column("IsActive", sa.Boolean(), nullable=False),
        sa.Column("BusinessId", sa.Integer(), nullable=True),
        sa.ForeignKeyConstraint(["BusinessId"], ["Business.Id"], name="fk_Suppliers_BusinessId_Business", ondelete="RESTRICT"),
        sa.PrimaryKeyConstraint("Id", name="pk_Suppliers"),
        sa.UniqueConstraint("BusinessId", "Email", name="uq_suppliers_business_id_email"),
        sa.UniqueConstraint("Id", "BusinessId", name="uq_suppliers_id_business_id"),
    )
    op.create_index("ix_Suppliers_BusinessId", "Suppliers", ["BusinessId"])
    op.create_table(
        "Products",
        sa.Column("Id", sa.Integer(), sa.Identity(), nullable=False),
        sa.Column("Name", sa.String(120), nullable=False),
        sa.Column("Description", sa.String(500), nullable=True),
        sa.Column("Sku", sa.String(40), nullable=False),
        sa.Column("Category", sa.String(80), nullable=False),
        sa.Column("Price", sa.Numeric(18, 2), nullable=False),
        sa.Column("StockQuantity", sa.Integer(), nullable=False),
        sa.Column("IsActive", sa.Boolean(), nullable=False),
        sa.Column("CreatedAtUtc", sa.DateTime(timezone=True), nullable=False),
        sa.Column("UpdatedAtUtc", sa.DateTime(timezone=True), nullable=True),
        sa.Column("SupplierId", sa.Integer(), nullable=True),
        sa.Column("CategoryId", sa.Integer(), nullable=True),
        sa.Column("BusinessId", sa.Integer(), nullable=True),
        sa.ForeignKeyConstraint(["BusinessId"], ["Business.Id"], name="fk_Products_BusinessId_Business"),
        sa.ForeignKeyConstraint(["CategoryId"], ["Categories.Id"], name="fk_products_category", ondelete="RESTRICT"),
        sa.ForeignKeyConstraint(["SupplierId"], ["Suppliers.Id"], name="fk_products_supplier", ondelete="RESTRICT"),
        sa.PrimaryKeyConstraint("Id", name="pk_Products"),
        sa.UniqueConstraint("Sku", name="uq_products_sku"),
    )
    op.create_index("ix_Products_BusinessId", "Products", ["BusinessId"])
    op.create_index("ix_Products_CategoryId", "Products", ["CategoryId"])
    op.create_index("ix_Products_SupplierId", "Products", ["SupplierId"])
    op.create_table(
        "OrderItems",
        sa.Column("Id", sa.Integer(), sa.Identity(), nullable=False),
        sa.Column("OrderId", sa.Integer(), nullable=False),
        sa.Column("ProductId", sa.Integer(), nullable=False),
        sa.Column("Quantity", sa.Integer(), nullable=False),
        sa.Column("UnitPrice", sa.Numeric(18, 2), nullable=False),
        sa.ForeignKeyConstraint(["OrderId"], ["Orders.Id"], name="fk_OrderItems_OrderId_Orders", ondelete="RESTRICT"),
        sa.ForeignKeyConstraint(["ProductId"], ["Products.Id"], name="fk_OrderItems_ProductId_Products", ondelete="RESTRICT"),
        sa.PrimaryKeyConstraint("Id", name="pk_OrderItems"),
    )
    op.create_index("ix_OrderItems_OrderId", "OrderItems", ["OrderId"])
    op.create_index("ix_OrderItems_ProductId", "OrderItems", ["ProductId"])


def downgrade() -> None:
    raise RuntimeError("The initial StockPilot schema is irreversible; no tables or data will be dropped automatically.")