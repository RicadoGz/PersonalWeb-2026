import json

from django.contrib.auth import get_user_model
from django.test import TestCase


class LoginApiTests(TestCase):
    def setUp(self):
        # create a user
        # get_user_model() to get current model user
        # create_user() to create a new user with the specified username, password, and email.
        self.user = get_user_model().objects.create_user(
            username="demo-admin",
            password="safe-password-123",
            email="demo@example.com",
        )

    def post_login(self, payload):
        # log in with the given payload by sending a POST request to the login endpoint.
        return self.client.post(
            "/api/login/",
            data=json.dumps(payload),
            content_type="application/json",
        )

    def post_logout(self):
        # send a POST request to the logout endpoint.
        return self.client.post("/api/logout/")

    def test_login_success(self):
        # if all the credentials are correct, the login should succeed and return the expected response.
        response = self.post_login(
            {"username": "demo-admin", "password": "safe-password-123"}
        )

        # when success return 200 and the response should indicate that the user is authenticated, along with the correct username.
        self.assertEqual(response.status_code, 200)
        self.assertEqual(response.json()["authenticated"], True)
        self.assertEqual(response.json()["username"], "demo-admin")

    def test_login_fails_with_wrong_password(self):
        # when wrogn should return this
        response = self.post_login(
            {"username": "demo-admin", "password": "wrong-password"}
        )

        # given the wrong password, the login should fail and return a 400 status code, with an error message indicating invalid credentials.
        self.assertEqual(response.status_code, 400)
        self.assertEqual(response.json()["authenticated"], False)
        self.assertEqual(response.json()["error"], "Invalid username or password")

    def test_logout_success_after_login(self):
        # first log in successfully, then call logout and expect the session to be cleared.
        self.post_login({"username": "demo-admin", "password": "safe-password-123"})

        response = self.post_logout()

        # logout should return 200 and mark the user as unauthenticated.
        self.assertEqual(response.status_code, 200)
        self.assertEqual(response.json()["authenticated"], False)
        self.assertEqual(response.json()["message"], "Logged out successfully")

    def test_logout_returns_safe_response_when_not_logged_in(self):
        # even if the user is not logged in, logout should still return a safe success response.
        response = self.post_logout()

        self.assertEqual(response.status_code, 200)
        self.assertEqual(response.json()["authenticated"], False)
        self.assertEqual(response.json()["message"], "Logged out successfully")
