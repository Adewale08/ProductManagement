from fastapi import APIRouter, Depends, Response, status
from sqlalchemy.orm import Session

from app.database import get_db
from app.dependencies.auth import get_current_business
from app.models import Business
from app.schemas.catalog import SupplierResponse, SupplierUpdate, SupplierWrite
from app.services import catalog

router = APIRouter(prefix="/api/Suppliers", tags=["suppliers"])


@router.get("", response_model=list[SupplierResponse])
def list_all(db: Session = Depends(get_db), business: Business = Depends(get_current_business)):
    return catalog.list_suppliers(db, business)


@router.get("/{supplier_id}", response_model=SupplierResponse)
def get_one(supplier_id: int, db: Session = Depends(get_db), business: Business = Depends(get_current_business)):
    return catalog.get_supplier(db, business, supplier_id)


@router.post("", response_model=SupplierResponse, status_code=status.HTTP_201_CREATED)
def create(request: SupplierWrite, response: Response, db: Session = Depends(get_db), business: Business = Depends(get_current_business)):
    supplier = catalog.create_supplier(db, business, request)
    response.headers["Location"] = f"/api/Suppliers/{supplier['id']}"
    return supplier


@router.put("/{supplier_id}", status_code=status.HTTP_204_NO_CONTENT)
def update(supplier_id: int, request: SupplierUpdate, db: Session = Depends(get_db), business: Business = Depends(get_current_business)):
    catalog.update_supplier(db, business, supplier_id, request)
    return Response(status_code=status.HTTP_204_NO_CONTENT)


@router.delete("/{supplier_id}", status_code=status.HTTP_204_NO_CONTENT)
def delete(supplier_id: int, db: Session = Depends(get_db), business: Business = Depends(get_current_business)):
    catalog.delete_supplier(db, business, supplier_id)
    return Response(status_code=status.HTTP_204_NO_CONTENT)