from django.urls import include, path

from apps.auth_api.views import dashboard_page, home_page, login_page, logout_page


urlpatterns = [
    path("", home_page, name="home_page"),
    path("login/", login_page, name="login_page"),
    path("dashboard/", dashboard_page, name="dashboard_page"),
    path("logout/", logout_page, name="logout_page"),
    path("api/", include("apps.auth_api.urls")),
]
