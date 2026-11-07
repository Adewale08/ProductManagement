from datetime import datetime
from decimal import Decimal

from pydantic import BaseModel, ConfigDict, EmailStr, Field


class InputModel(BaseModel):
    model_config = ConfigDict(populate_by_name=True)


class ProductWrite(InputModel):
    name: str = Field(min_length=1, max_length=120)
    description: str | None = Field(default=None, max_length=500)
    sku: str = Field(min_length=1, max_length=40)
    category: str = Field(min_length=1, max_length=80)
    category_id: int | None = Field(default=None, alias="categoryId", ge=1)
    supplier_id: int | None = Field(default=None, alias="supplierId", ge=1)
    price: Decimal = Field(ge=0, le=999_999_999)
    stock_quantity: int = Field(alias="stockQuantity", ge=0, le=2_147_483_647)


class ProductUpdate(ProductWrite):
    is_active: bool = Field(default=True, alias="isActive")


class StockUpdate(InputModel):
    stock_quantity: int = Field(alias="stockQuantity", ge=0, le=2_147_483_647)


class ProductResponse(BaseModel):
    id: int
    name: str
    description: str | None
    sku: str
    category: str
    price: float
    stock_quantity: int = Field(alias="stockQuantity")
    is_active: bool = Field(alias="isActive")
    created_at_utc: datetime = Field(alias="createdAtUtc")
    updated_at_utc: datetime | None = Field(alias="updatedAtUtc")
    category_id: int | None = Field(alias="categoryId")
    supplier_id: int | None = Field(alias="supplierId")

    model_config = ConfigDict(serialize_by_alias=True, populate_by_name=True)


class CategoryWrite(InputModel):
    name: str = Field(min_length=1, max_length=100)
    description: str | None = Field(default=None, max_length=250)


class CategoryUpdate(CategoryWrite):
    is_active: bool = Field(default=True, alias="isActive")


class CategoryQuery(BaseModel):
    name: str | None = Field(default=None, max_length=100)
    is_active: bool | None = Field(default=None, alias="isActive")
    page: int = 1
    page_size: int = Field(default=10, alias="pageSize")


class CategoryResponse(BaseModel):
    id: int
    name: str
    description: str | None
    is_active: bool = Field(alias="isActive")
    created_at_utc: datetime = Field(alias="createdAtUtc")
    updated_at_utc: datetime | None = Field(alias="updatedAtUtc")

    model_config = ConfigDict(serialize_by_alias=True, populate_by_name=True)


class SupplierWrite(InputModel):
    name: str = Field(min_length=1, max_length=120)
    email: EmailStr = Field(max_length=254)
    phone_number: str | None = Field(default=None, alias="phoneNumber", max_length=30)


class SupplierUpdate(SupplierWrite):
    is_active: bool = Field(default=True, alias="isActive")


class SupplierResponse(BaseModel):
    id: int
    name: str
    email: str
    phone_number: str | None = Field(alias="phoneNumber")
    is_active: bool = Field(alias="isActive")

    model_config = ConfigDict(serialize_by_alias=True, populate_by_name=True)