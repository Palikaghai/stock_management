from django.contrib.auth import views as auth_views
from django.urls import path

from . import views

urlpatterns = [
    path('', views.dashboard, name='dashboard'),
    path('login/', auth_views.LoginView.as_view(template_name='registration/login.html'), name='login'),
    path('logout/', auth_views.LogoutView.as_view(), name='logout'),
    path('products/', views.product_list, name='product-list'),
    path('products/new/', views.product_create, name='product-create'),
    path('products/<int:pk>/edit/', views.product_update, name='product-update'),
    path('products/<int:pk>/delete/', views.product_delete, name='product-delete'),
    path('categories/', views.category_list, name='category-list'),
    path('categories/new/', views.category_create, name='category-create'),
    path('categories/<int:pk>/delete/', views.category_delete, name='category-delete'),
    path('suppliers/', views.supplier_list, name='supplier-list'),
    path('suppliers/new/', views.supplier_create, name='supplier-create'),
    path('suppliers/<int:pk>/delete/', views.supplier_delete, name='supplier-delete'),
    path('stock/', views.stock_operation, name='stock-operation'),
    path('history/', views.transaction_history, name='transaction-history'),
    path('reports/', views.reports, name='reports'),
]
