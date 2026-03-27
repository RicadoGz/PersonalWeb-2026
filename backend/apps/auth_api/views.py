import json

from django.contrib.auth import authenticate, login, logout
from django.contrib.auth.decorators import login_required
from django.http import JsonResponse
from django.shortcuts import redirect, render
from django.views.decorators.csrf import csrf_exempt
from django.views.decorators.http import require_GET
from django.views.decorators.http import require_POST


def parse_json_body(request):
    # 前端如果传的是 JSON，这里把 request.body 安全地解析成 Python 字典。
    # 如果 JSON 格式不合法，就返回空字典，避免整个接口直接报错。
    try:
        return json.loads(request.body or "{}")
    except json.JSONDecodeError:
        return {}


def authenticate_user(request, username, password):
    # 统一处理用户名清理和 Django 认证，避免 API 和页面登录逻辑重复。
    cleaned_username = str(username).strip()
    cleaned_password = str(password)

    if not cleaned_username or not cleaned_password:
        return None, cleaned_username, "Username and password are required"

    user = authenticate(request, username=cleaned_username, password=cleaned_password)
    if not user:
        return None, cleaned_username, "Invalid username or password"

    return user, cleaned_username, None


@csrf_exempt #不检查csrf token
@require_POST #只允许POST请求访问这个接口
def login_view(request):
    # 先把请求体里的 JSON 取出来。
    payload = parse_json_body(request)
    user, username, error_message = authenticate_user(
        request,
        payload.get("username", ""),
        payload.get("password", ""),
    )
    if error_message:
        return JsonResponse(
            {
                "authenticated": False,
                "error": error_message,
            },
            status=400,
        )

    # login() 会把这个用户写进当前 session。
    # 后面这个浏览器再发请求时，服务端就知道“这个人已经登录了”。
    login(request, user)

    # 登录成功后返回最小必要信息。
    # 这里先只返回 authenticated 和 username，后面需要的话再扩展。
    return JsonResponse(
        {
            "authenticated": True,
            "username": user.username,
        }
    )


@require_GET
def home_page(request):
    # 首页只做最小导航，让 Selenium 有一个稳定的落点。
    return render(request, "auth_api/home.html")


def login_page(request):
    # 已登录用户直接跳到 dashboard，避免重复登录。
    if request.user.is_authenticated:
        return redirect("dashboard_page")

    if request.method == "POST":
        user, username, error_message = authenticate_user(
            request,
            request.POST.get("username", ""),
            request.POST.get("password", ""),
        )
        if error_message:
            return render(
                request,
                "auth_api/login.html",
                {"error": error_message, "username": username},
                status=400,
            )

        login(request, user)
        return redirect("dashboard_page")

    return render(request, "auth_api/login.html")


@login_required(login_url="/login/")
def dashboard_page(request):
    # 登录后的最小页面，用来练 Selenium 的状态断言和 logout 流程。
    return render(request, "auth_api/dashboard.html")


@require_POST
def logout_page(request):
    # 页面登出成功后回到登录页。
    logout(request)
    return redirect("login_page")


@csrf_exempt  # 不检查 csrf token
@require_POST  # 只允许 POST 请求访问这个接口
def logout_view(request):
    # 不管当前用户有没有登录，都先调用 Django 的 logout 清理 session。
    logout(request)

    # 第一版保持简单，统一返回退出成功的响应。
    return JsonResponse(
        {
            "authenticated": False,
            "message": "Logged out successfully",
        }
    )
