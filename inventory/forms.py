from django import forms

from .models import Category, Product, StockTransaction, Supplier


class ProductForm(forms.ModelForm):
    class Meta:
        model = Product
        fields = ['sku', 'name', 'category', 'supplier', 'description', 'unit_price', 'quantity', 'reorder_level']
        widgets = {'description': forms.Textarea(attrs={'rows': 3})}


class CategoryForm(forms.ModelForm):
    class Meta:
        model = Category
        fields = ['name', 'description']
        widgets = {'description': forms.Textarea(attrs={'rows': 3})}


class SupplierForm(forms.ModelForm):
    class Meta:
        model = Supplier
        fields = ['name', 'contact_person', 'email', 'phone', 'address']
        widgets = {'address': forms.Textarea(attrs={'rows': 3})}


class StockTransactionForm(forms.ModelForm):
    class Meta:
        model = StockTransaction
        fields = ['product', 'transaction_type', 'quantity', 'note']
        widgets = {'note': forms.TextInput(attrs={'placeholder': 'Optional reason or reference'})}
