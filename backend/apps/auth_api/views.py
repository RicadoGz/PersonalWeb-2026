import json

from django.contrib.auth import authenticate, login, logout
from django.http import JsonResponse
from django.views.decorators.csrf import csrf_exempt
from django.views.decorators.http import require_POST


def parse_json_body(request):
    # 前端如果传的是 JSON，这里把 request.body 安全地解析成 Python 字典。
    # 如果 JSON 格式不合法，就返回空字典，避免整个接口直接报错。
    try:
        return json.loads(request.body or "{}")
    except json.JSONDecodeError:
        return {}


@csrf_exempt #不检查csrf token
@require_POST #只允许POST请求访问这个接口
def login_view(request):
    # 先把请求体里的 JSON 取出来。
    payload = parse_json_body(request)

    # 读取 username 和 password。
    # username 这里额外做了 strip()，可以自动去掉前后空格。
    username = str(payload.get("username", "")).strip()
    password = str(payload.get("password", ""))

    # 只要有一个字段为空，就直接返回 400。
    # 这是最小的输入校验，避免把空值继续交给认证逻辑。
    if not username or not password:
        return JsonResponse(
            {
                "authenticated": False,
                "error": "Username and password are required",
            },
            status=400,
        )

    # authenticate() 是 Django 自带的认证函数。
    # 它会去数据库里查这个用户名和密码是否匹配。
    # 匹配成功就返回 user 对象，失败就返回 None。
    user = authenticate(request, username=username, password=password)
    if not user:
        return JsonResponse(
            {
                "authenticated": False,
                "error": "Invalid username or password",
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
