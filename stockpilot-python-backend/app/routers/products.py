from fastapi import APIRouter, Depends, Response, status
from sqlalchemy.orm import Session

from app.database import get_db
from app.dependencies.auth import get_current_business
from app.models import Business
from app.schemas.catalog import ProductResponse, ProductUpdate, ProductWrite, StockUpdate
from app.services import catalog

router = APIRouter(prefix="/Products", tags=["products"])


@router.get("", response_model=list[ProductResponse])
def list_all(db: Session = Depends(get_db), business: Business = Depends(get_current_business)):
    return catalog.list_products(db, business)


@router.get("/{product_id}", response_model=ProductResponse)
def get_one(product_id: int, db: Session = Depends(get_db), business: Business = Depends(get_current_business)):
    return catalog.get_product(db, business, product_id)


@router.post("", response_model=ProductResponse, status_code=status.HTTP_201_CREATED)
def create(request: ProductWrite, response: Response, db: Session = Depends(get_db), business: Business = Depends(get_current_business)):
    product = catalog.create_product(db, business, request)
    response.headers["Location"] = f"/Products/{product['id']}"
    return product


@router.put("/{product_id}", status_code=status.HTTP_204_NO_CONTENT)
def update(product_id: int, request: ProductUpdate, db: Session = Depends(get_db), business: Business = Depends(get_current_business)):
    catalog.update_product(db, business, product_id, request)
    return Response(status_code=status.HTTP_204_NO_CONTENT)


@router.delete("/{product_id}", status_code=status.HTTP_204_NO_CONTENT)
def delete(product_id: int, db: Session = Depends(get_db), business: Business = Depends(get_current_business)):
    catalog.delete_product(db, business, product_id)
    return Response(status_code=status.HTTP_204_NO_CONTENT)


@router.patch("/{product_id}/stock", status_code=status.HTTP_204_NO_CONTENT)
def update_stock(product_id: int, request: StockUpdate, db: Session = Depends(get_db), business: Business = Depends(get_current_business)):
    catalog.set_stock(db, business, product_id, request)
    return Response(status_code=status.HTTP_204_NO_CONTENT)