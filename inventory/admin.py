from django.contrib import admin

from .models import Category, Product, StockTransaction, Supplier


@admin.register(Product)
class ProductAdmin(admin.ModelAdmin):
    list_display = ('sku', 'name', 'category', 'quantity', 'unit_price', 'supplier')
    list_filter = ('category', 'supplier')
    search_fields = ('sku', 'name')


@admin.register(StockTransaction)
class StockTransactionAdmin(admin.ModelAdmin):
    list_display = ('product', 'transaction_type', 'quantity', 'created_by', 'created_at')
    list_filter = ('transaction_type', 'created_at')
    search_fields = ('product__name', 'product__sku')


admin.site.register(Category)
admin.site.register(Supplier)
