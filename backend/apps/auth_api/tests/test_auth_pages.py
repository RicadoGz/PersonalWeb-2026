from django.contrib.auth import get_user_model
from django.test import TestCase


class AuthPageTests(TestCase):
    def setUp(self):
        self.user = get_user_model().objects.create_user(
            username="demo-admin",
            password="safe-password-123",
            email="demo@example.com",
        )

    def test_login_page_renders(self):
        response = self.client.get("/login/")

        self.assertEqual(response.status_code, 200)
        self.assertContains(response, "Login")
        self.assertContains(response, 'id="login-form"', html=False)

    def test_login_page_redirects_to_dashboard_after_successful_login(self):
        response = self.client.post(
            "/login/",
            {"username": "demo-admin", "password": "safe-password-123"},
        )

        self.assertRedirects(response, "/dashboard/")

    def test_dashboard_requires_login(self):
        response = self.client.get("/dashboard/")

        self.assertRedirects(response, "/login/?next=/dashboard/")

    def test_logout_page_redirects_back_to_login(self):
        self.client.login(username="demo-admin", password="safe-password-123")

        response = self.client.post("/logout/")

        self.assertRedirects(response, "/login/")
