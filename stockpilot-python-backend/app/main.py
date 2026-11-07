import logging

from fastapi import FastAPI, Request
from fastapi.exceptions import RequestValidationError
from fastapi.middleware.cors import CORSMiddleware
from fastapi.responses import JSONResponse

from app.core.config import get_settings
from app.core.errors import APIError
from app.routers.auth import router as auth_router
from app.routers.categories import router as categories_router
from app.routers.orders import router as orders_router
from app.routers.products import router as products_router
from app.routers.suppliers import router as suppliers_router

logging.basicConfig(level=logging.INFO, format="%(asctime)s %(levelname)s %(name)s %(message)s")
logger = logging.getLogger("stockpilot.api")
settings = get_settings()

app = FastAPI(title="StockPilot API", version="0.3.0")

if settings.cors_origins:
    app.add_middleware(
        CORSMiddleware,
        allow_origins=settings.cors_origins,
        allow_credentials=True,
        allow_methods=["GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS"],
        allow_headers=["Authorization", "Content-Type"],
    )

app.include_router(auth_router)
app.include_router(products_router)
app.include_router(categories_router)
app.include_router(suppliers_router)
app.include_router(orders_router)


@app.middleware("http")
async def check_browser_write_origin(request: Request, call_next):
    if request.method in {"POST", "PUT", "PATCH", "DELETE"}:
        origin = request.headers.get("origin")
        allowed = {value.lower() for value in settings.cors_origins}
        request_origin = f"{request.url.scheme}://{request.url.netloc}".lower().rstrip("/")
        same_origin = bool(origin) and origin.lower().rstrip("/") == request_origin
        if origin and not same_origin and origin.lower().rstrip("/") not in allowed:
            return JSONResponse(
                status_code=403,
                content={"status": 403, "message": "Requests from this origin are not allowed."},
            )
    return await call_next(request)


@app.exception_handler(APIError)
async def api_error_handler(_request: Request, exc: APIError) -> JSONResponse:
    return JSONResponse(status_code=exc.status_code, content={"status": exc.status_code, "message": exc.message})


@app.exception_handler(RequestValidationError)
async def request_validation_error(_request: Request, exc: RequestValidationError) -> JSONResponse:
    errors: dict[str, list[str]] = {}
    for error in exc.errors():
        location = error.get("loc", ())
        field = ".".join(str(part) for part in location if part not in {"body", "query", "path"}) or "request"
        errors.setdefault(field, []).append(error.get("msg", "Invalid value."))
    return JSONResponse(
        status_code=400,
        content={"message": "One or more validation errors occurred.", "errors": errors},
    )


@app.exception_handler(Exception)
async def unexpected_error_handler(_request: Request, exc: Exception) -> JSONResponse:
    logger.error("Unexpected API error: %s", type(exc).__name__)
    return JSONResponse(status_code=500, content={"status": 500, "message": "An unexpected error occurred."})


@app.get("/health", tags=["health"])
def health() -> dict[str, str]:
    return {"status": "ok"}
