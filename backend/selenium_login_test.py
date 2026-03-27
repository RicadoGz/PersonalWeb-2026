from selenium import webdriver
from selenium.webdriver.common.by import By
from selenium.webdriver.support import expected_conditions as EC
from selenium.webdriver.support.ui import WebDriverWait


BASE_URL = "http://127.0.0.1:8000"
TEST_USERNAME = "demo-admin"
TEST_PASSWORD = "safe-password-123"
INVALID_TEST_PASSWORD = "wrong-password"


def create_driver_and_wait():
    # Create a fresh browser instance for a single Selenium test scenario.
    browser_driver = webdriver.Chrome()
    wait = WebDriverWait(browser_driver, 10)
    return browser_driver, wait


def login_through_form(driver, wait):
    # Open the login page in the browser.
    driver.get(f"{BASE_URL}/login/")

    # Wait until the username input is present, so we know the page is ready.
    username_input = wait.until(EC.presence_of_element_located((By.ID, "username")))

    # Find the password input and the login button on the page.
    password_input = driver.find_element(By.ID, "password")
    login_button = driver.find_element(By.ID, "login-submit")

    # Type the valid username and password into the login form.
    username_input.send_keys(TEST_USERNAME)
    password_input.send_keys(TEST_PASSWORD)

    # Submit the form by clicking the login button.
    login_button.click()

    # Wait until the browser has moved to the dashboard page.
    wait.until(EC.url_contains("/dashboard/"))


def run_login_test(driver, wait):
    # Reuse the shared login steps to complete a successful login.
    login_through_form(driver, wait)

    # Wait for the welcome message on the dashboard to appear.
    dashboard_welcome_message = wait.until(
        EC.presence_of_element_located((By.ID, "welcome-message"))
    )

    # Verify that a successful login redirected the browser to the dashboard page.
    assert "/dashboard/" in driver.current_url, (
        "The login flow should redirect the browser to the dashboard page."
    )

    # Verify that the dashboard shows the logged-in user's username.
    assert TEST_USERNAME in dashboard_welcome_message.text, (
        "The dashboard welcome message should include the logged-in username."
    )

    print("Selenium login test passed.")


def run_logout_test(driver, wait):
    # Start from a logged-in state so the logout button is available.
    login_through_form(driver, wait)

    # Wait until the logout button can be clicked, then click it.
    logout_button = wait.until(EC.element_to_be_clickable((By.ID, "logout-submit")))
    logout_button.click()

    # Wait until the browser returns to the login page.
    wait.until(EC.url_contains("/login/"))
    login_page_title = wait.until(
        EC.presence_of_element_located((By.ID, "login-title"))
    )

    # Verify that logout redirected the browser back to the login page.
    assert "/login/" in driver.current_url, (
        "The logout flow should return the browser to the login page."
    )

    # Verify that the login page loaded correctly after logout.
    assert login_page_title.text == "Login", (
        "The login page title should be visible after the logout flow completes."
    )

    print("Selenium logout test passed.")


def run_login_failure_test(driver, wait):
    # Open the login page directly for the failed-login scenario.
    driver.get(f"{BASE_URL}/login/")

    # Wait for the login form elements so we can interact with them.
    username_input = wait.until(EC.presence_of_element_located((By.ID, "username")))
    password_input = driver.find_element(By.ID, "password")
    login_button = driver.find_element(By.ID, "login-submit")

    # Enter a valid username but an invalid password to trigger a login failure.
    username_input.send_keys(TEST_USERNAME)
    password_input.send_keys(INVALID_TEST_PASSWORD)

    # Submit the invalid login attempt.
    login_button.click()

    # Wait for the error message to appear on the page.
    login_error_message = wait.until(
        EC.presence_of_element_located((By.ID, "login-error"))
    )

    # Verify that the browser stays on the login page after a failed login attempt.
    assert "/login/" in driver.current_url, (
        "A failed login attempt should keep the browser on the login page."
    )

    # Verify that the page shows the expected invalid-credentials error message.
    assert login_error_message.text == "Invalid username or password", (
        "The failed login flow should display the invalid username or password message."
    )

    print("Selenium failed-login test passed.")


def run_all():
    # Run each scenario in a separate browser session so one test does not affect another.
    browser_driver, wait = create_driver_and_wait()
    try:
        run_login_test(browser_driver, wait)
    finally:
        browser_driver.quit()

    browser_driver, wait = create_driver_and_wait()
    try:
        run_logout_test(browser_driver, wait)
    finally:
        browser_driver.quit()

    browser_driver, wait = create_driver_and_wait()
    try:
        run_login_failure_test(browser_driver, wait)
    finally:
        browser_driver.quit()


if __name__ == "__main__":
    run_all()
