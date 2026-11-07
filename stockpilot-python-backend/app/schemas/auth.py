from pydantic import BaseModel, ConfigDict, EmailStr, Field


class RegisterRequest(BaseModel):


    email: EmailStr = Field(max_length=256)
    password: str = Field(min_length=6)
    business_name: str = Field(alias="businessName", min_length=3, max_length=30)
    full_name: str = Field(alias="fullName", min_length=2, max_length=120)


class LoginRequest(BaseModel):


    email: EmailStr = Field(max_length=256)
    password: str = Field(min_length=1)


class CurrentUserResponse(BaseModel):
    model_config = ConfigDict(serialize_by_alias=True)

    user_id: str = Field(serialization_alias="userId")
    business_name: str = Field(serialization_alias="businessName")
    email: str