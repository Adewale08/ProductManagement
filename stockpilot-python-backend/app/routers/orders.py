from fastapi import APIRouter, Depends, Response, status
from sqlalchemy.orm import Session

from app.database import get_db
from app.dependencies.auth import get_current_business
from app.models import Business
from app.schemas.orders import CreateOrder, OrderResponse, UpdateOrderStatus
from app.services import orders

router = APIRouter(prefix="/Orders", tags=["orders"])


@router.post("", response_model=OrderResponse, status_code=status.HTTP_201_CREATED)
def create(request: CreateOrder, response: Response, db: Session = Depends(get_db), business: Business = Depends(get_current_business)):
    order = orders.create_order(db, business, request)
    response.headers["Location"] = f"/Orders/{order['id']}"
    return order


@router.get("", response_model=list[OrderResponse])
def list_all(db: Session = Depends(get_db), business: Business = Depends(get_current_business)):
    return orders.list_orders(db, business)


@router.get("/{order_id}", response_model=OrderResponse)
def get_one(order_id: int, db: Session = Depends(get_db), business: Business = Depends(get_current_business)):
    return orders.get_order(db, business, order_id)


@router.patch("/{order_id}/status", response_model=OrderResponse)
def update_status(order_id: int, request: UpdateOrderStatus, db: Session = Depends(get_db), business: Business = Depends(get_current_business)):
    return orders.update_order_status(db, business, order_id, request)