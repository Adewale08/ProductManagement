from datetime import datetime
from decimal import Decimal

from pydantic import BaseModel, ConfigDict, Field


class CreateOrderItem(BaseModel):
    product_id: int = Field(alias="productId", ge=1, le=2_147_483_647)
    quantity: int = Field(ge=1, le=2_147_483_647)

    model_config = ConfigDict(populate_by_name=True)


class CreateOrder(BaseModel):
    items: list[CreateOrderItem] = Field(min_length=1)


class UpdateOrderStatus(BaseModel):
    status: str = Field(min_length=1)


class OrderItemResponse(BaseModel):
    product_id: int = Field(alias="productId")
    product_name: str = Field(alias="productName")
    quantity: int
    unit_price: float = Field(alias="unitPrice")
    line_total: float = Field(alias="lineTotal")

    model_config = ConfigDict(serialize_by_alias=True, populate_by_name=True)


class OrderResponse(BaseModel):
    id: int
    created_at_utc: datetime = Field(alias="createdAtUtc")
    status: str
    total_amount: float = Field(alias="totalAmount")
    items: list[OrderItemResponse]

    model_config = ConfigDict(serialize_by_alias=True, populate_by_name=True)