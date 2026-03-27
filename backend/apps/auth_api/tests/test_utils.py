import json

from django.test import RequestFactory, SimpleTestCase

from ..views import parse_json_body


class ParseJsonBodyUnitTests(SimpleTestCase):
    def setUp(self):
        # 用 RequestFactory make a fake request
        self.factory = RequestFactory()

    def test_parse_json_body_returns_dict_for_valid_json(self):
        # to test that parse_json_body correctly parses a valid JSON request body into a Python dictionary.
        request = self.factory.post(
            "/api/login/",
            data=json.dumps({"username": "demo-admin", "password": "safe-password-123"}),
            content_type="application/json",
        )

        self.assertEqual(
            parse_json_body(request),
            {"username": "demo-admin", "password": "safe-password-123"},
        )

    def test_parse_json_body_returns_empty_dict_for_invalid_json(self):
        # when the invalid input with json they return an empty dictionary instead of throwing an error.
        request = self.factory.post(
            "/api/login/",
            data='{"username": "demo-admin", "password": ',
            content_type="application/json",
        )

        self.assertEqual(parse_json_body(request), {})

    def test_parse_json_body_returns_empty_dict_for_empty_body(self):
        # when the request body is empty, it should be safely parsed into an empty dictionary.
        request = self.factory.post(
            "/api/login/",
            data="",
            content_type="application/json",
        )

        self.assertEqual(parse_json_body(request), {})
