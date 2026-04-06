# Frontend Reading Order / 前端阅读顺序

## Goal / 目标

This document explains the best reading order for the current Blazor frontend.

这份文档说明当前这版 Blazor 前端最适合的阅读顺序。

The purpose is simple:

目的很简单：

- understand what each file does
- know which files matter most
- avoid getting lost in the template structure

- 理解每个文件的作用
- 知道哪些文件最重要
- 避免一开始就陷进模板结构里

---

## Best Reading Order / 最佳阅读顺序

### 1. `Home.razor`

File:
[Home.razor](/Users/gz/Desktop/coding/personalWeb/frontend/PersonalWeb.Web/Components/Pages/Home.razor)

Read this first.

先读这个。

Why:

原因：

- this is the homepage
- this defines the first impression of the portfolio
- this contains the hero section, projects, experience, and contact summary

- 这是首页
- 它决定了整个作品集的第一印象
- 它包含了首屏、项目区、经历区和联系信息

What to focus on:

阅读重点：

- top hero structure
- project section
- experience section
- page content order

- 首屏结构
- 项目区
- 经历区
- 整个页面的信息排列顺序

---

### 2. `HelloRotator.razor`

File:
[HelloRotator.razor](/Users/gz/Desktop/coding/personalWeb/frontend/PersonalWeb.Web/Components/HelloRotator.razor)

Read this second.

第二个读这个。

Why:

原因：

- this is the small interactive greeting component
- it rotates different “Hi” greetings using Blazor state updates
- it is a good example of simple interactive UI logic

- 这是首屏的小交互组件
- 它通过 Blazor 状态更新来轮播不同语言的 “Hi”
- 它是一个很好的轻量交互逻辑示例

What to focus on:

阅读重点：

- `Timer`
- `currentIndex`
- `StateHasChanged`
- `Dispose`

- `Timer`
- `currentIndex`
- `StateHasChanged`
- `Dispose`

---

### 3. `MainLayout.razor`

File:
[MainLayout.razor](/Users/gz/Desktop/coding/personalWeb/frontend/PersonalWeb.Web/Components/Layout/MainLayout.razor)

Read this after the homepage.

看完首页后读这个。

Why:

原因：

- this wraps all pages
- it defines the shared page frame
- it also mounts the floating AI widget

- 它包住所有页面
- 它定义了页面共用外框
- 它也挂载了右下角 AI 浮窗

What to focus on:

阅读重点：

- `@Body`
- header structure
- `FloatingAgentWidget`

- `@Body`
- 顶部结构
- `FloatingAgentWidget`

---

### 4. `FloatingAgentWidget.razor`

File:
[FloatingAgentWidget.razor](/Users/gz/Desktop/coding/personalWeb/frontend/PersonalWeb.Web/Components/FloatingAgentWidget.razor)

Read this next if you want to understand the chatbot entry.

如果你想理解右下角小聊天入口，这个接着读。

Why:

原因：

- this controls the floating chatbot launcher
- this controls open and close state
- this shows how a small interactive panel works in Blazor

- 它控制右下角聊天入口
- 它控制打开/关闭状态
- 它展示了一个小型交互面板在 Blazor 里怎么写

What to focus on:

阅读重点：

- `isOpen`
- `Toggle()`
- launcher button
- popup panel structure

- `isOpen`
- `Toggle()`
- 入口按钮
- 弹窗结构

---

### 5. `Login.razor`

File:
[Login.razor](/Users/gz/Desktop/coding/personalWeb/frontend/PersonalWeb.Web/Components/Pages/Login.razor)

Read this after understanding the homepage.

理解首页之后再看这个。

Why:

原因：

- this is the dedicated login page
- this page is simpler than the homepage
- it shows how page routing is separated from reusable form logic

- 这是独立登录页
- 这个页面比首页简单
- 它能帮助你理解页面路由和可复用表单逻辑是如何分开的

What to focus on:

阅读重点：

- route `/login`
- login page structure
- how it uses `LoginFormCard`

- `/login` 路由
- 登录页结构
- 它如何使用 `LoginFormCard`

---

### 6. `LoginFormCard.razor`

File:
[LoginFormCard.razor](/Users/gz/Desktop/coding/personalWeb/frontend/PersonalWeb.Web/Components/LoginFormCard.razor)

This is the most important file for frontend-backend interaction.

这是前后端交互最重要的文件。

Why:

原因：

- this contains the login form
- this calls the backend API
- this is where `HttpClientFactory` is actually used

- 这里有登录表单
- 这里会调用后端 API
- 这里是真正使用 `HttpClientFactory` 的地方

What to focus on:

阅读重点：

- `@inject IHttpClientFactory HttpClientFactory`
- `EditForm`
- `HandleLoginAsync()`
- `CreateClient("BackendApi")`
- `PostAsJsonAsync(...)`

- `@inject IHttpClientFactory HttpClientFactory`
- `EditForm`
- `HandleLoginAsync()`
- `CreateClient("BackendApi")`
- `PostAsJsonAsync(...)`

---

### 7. `Program.cs`

File:
[Program.cs](/Users/gz/Desktop/coding/personalWeb/frontend/PersonalWeb.Web/Program.cs)

Read this after you already know the visible pages.

在你已经知道页面长什么样之后，再读这个。

Why:

原因：

- this is the app startup file
- it registers services
- it configures the backend API client

- 这是应用启动入口
- 它注册服务
- 它配置后端 API 客户端

What to focus on:

阅读重点：

- `AddRazorComponents()`
- `AddInteractiveServerComponents()`
- `AddHttpClient("BackendApi", ...)`
- `MapRazorComponents<App>()`

- `AddRazorComponents()`
- `AddInteractiveServerComponents()`
- `AddHttpClient("BackendApi", ...)`
- `MapRazorComponents<App>()`

---

### 8. `Routes.razor`

File:
[Routes.razor](/Users/gz/Desktop/coding/personalWeb/frontend/PersonalWeb.Web/Components/Routes.razor)

Read this when you want to understand how `/` and `/login` are connected.

当你想理解 `/` 和 `/login` 是怎么连起来的时候读这个。

Why:

原因：

- this controls route matching
- this decides which page component gets rendered

- 它控制路由匹配
- 它决定显示哪个页面组件

What to focus on:

阅读重点：

- `Router`
- `RouteView`
- `DefaultLayout`

- `Router`
- `RouteView`
- `DefaultLayout`

---

### 9. `App.razor`

File:
[App.razor](/Users/gz/Desktop/coding/personalWeb/frontend/PersonalWeb.Web/Components/App.razor)

Read this near the end.

这个放到后面读。

Why:

原因：

- it is the app shell
- it includes head, styles, and route outlet
- it is important, but not the best first file for learning the UI

- 它是应用总外壳
- 它包含 head、样式和路由出口
- 它很重要，但不适合作为理解 UI 的第一份文件

---

### 10. `app.css`

File:
[app.css](/Users/gz/Desktop/coding/personalWeb/frontend/PersonalWeb.Web/wwwroot/app.css)

Read this last.

最后再看这个。

Why:

原因：

- it is large
- it controls many visual layers
- it is easier to understand once you already know the components

- 它很大
- 它控制了很多视觉层
- 先知道组件结构，再看它会轻松很多

What to focus on:

阅读重点：

- hero and intro styles
- project card styles
- floating chatbot styles
- login styles

- 首屏与介绍区域样式
- 项目卡片样式
- 浮动聊天组件样式
- 登录页样式

---

## Short Version / 最短版本

If you only want the fastest useful reading path:

如果你只想走最快的有效阅读路线：

1. [Home.razor](/Users/gz/Desktop/coding/personalWeb/frontend/PersonalWeb.Web/Components/Pages/Home.razor)
2. [HelloRotator.razor](/Users/gz/Desktop/coding/personalWeb/frontend/PersonalWeb.Web/Components/HelloRotator.razor)
3. [MainLayout.razor](/Users/gz/Desktop/coding/personalWeb/frontend/PersonalWeb.Web/Components/Layout/MainLayout.razor)
4. [FloatingAgentWidget.razor](/Users/gz/Desktop/coding/personalWeb/frontend/PersonalWeb.Web/Components/FloatingAgentWidget.razor)
5. [LoginFormCard.razor](/Users/gz/Desktop/coding/personalWeb/frontend/PersonalWeb.Web/Components/LoginFormCard.razor)
6. [Program.cs](/Users/gz/Desktop/coding/personalWeb/frontend/PersonalWeb.Web/Program.cs)
7. [app.css](/Users/gz/Desktop/coding/personalWeb/frontend/PersonalWeb.Web/wwwroot/app.css)

---

## Best Way To Read / 最推荐的阅读方式

Do not try to understand everything at once.

不要一口气把所有东西都搞懂。

Use this method:

建议用这个方式：

1. read the page structure first
2. then read the small interactive component
3. then read the layout
4. then read the backend interaction file
5. finally read the styling

1. 先读页面结构
2. 再读小交互组件
3. 再读布局
4. 再读前后端交互文件
5. 最后再看样式

That is the fastest way to understand this frontend without getting lost.

这是在不迷路的前提下，最快看懂这个前端的方式。
