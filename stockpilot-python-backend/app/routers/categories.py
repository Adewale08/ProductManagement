from fastapi import APIRouter, Depends, Response, status
from sqlalchemy.orm import Session

from app.database import get_db
from app.dependencies.auth import get_current_business
from app.models import Business
from app.schemas.catalog import CategoryQuery, CategoryResponse, CategoryUpdate, CategoryWrite
from app.services import catalog

router = APIRouter(prefix="/Category", tags=["categories"])


@router.get("", response_model=list[CategoryResponse])
def list_all(query: CategoryQuery = Depends(), db: Session = Depends(get_db), business: Business = Depends(get_current_business)):
    return catalog.list_categories(db, business, query)


@router.get("/{category_id}", response_model=CategoryResponse)
def get_one(category_id: int, db: Session = Depends(get_db), business: Business = Depends(get_current_business)):
    return catalog.get_category(db, business, category_id)


@router.post("", response_model=CategoryResponse, status_code=status.HTTP_201_CREATED)
def create(request: CategoryWrite, response: Response, db: Session = Depends(get_db), business: Business = Depends(get_current_business)):
    category = catalog.create_category(db, business, request)
    response.headers["Location"] = f"/Category/{category['id']}"
    return category


@router.put("/{category_id}", status_code=status.HTTP_204_NO_CONTENT)
def update(category_id: int, request: CategoryUpdate, db: Session = Depends(get_db), business: Business = Depends(get_current_business)):
    catalog.update_category(db, business, category_id, request)
    return Response(status_code=status.HTTP_204_NO_CONTENT)


@router.delete("/{category_id}", status_code=status.HTTP_204_NO_CONTENT)
def delete(category_id: int, db: Session = Depends(get_db), business: Business = Depends(get_current_business)):
    catalog.delete_category(db, business, category_id)
    return Response(status_code=status.HTTP_204_NO_CONTENT)