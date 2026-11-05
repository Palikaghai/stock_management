from django.contrib import messages
from django.contrib.auth.decorators import login_required
from django.db import transaction
from django.db.models import Count, DecimalField, ExpressionWrapper, F, Q, Sum
from django.shortcuts import get_object_or_404, redirect, render
from django.utils import timezone

from .forms import CategoryForm, ProductForm, StockTransactionForm, SupplierForm
from .models import Category, Product, StockTransaction, Supplier


@login_required
def dashboard(request):
	products = Product.objects.select_related('category', 'supplier')
	stats = products.aggregate(
		total_products=Count('id'),
		stock_value=Sum(ExpressionWrapper(F('quantity') * F('unit_price'), output_field=DecimalField(max_digits=14, decimal_places=2))),
	)
	return render(request, 'inventory/dashboard.html', {
		'stats': stats,
		'today': timezone.localdate(),
		'low_stock': products.filter(quantity__lte=F('reorder_level'))[:6],
		'recent_transactions': StockTransaction.objects.select_related('product', 'created_by')[:6],
		'category_count': Category.objects.count(),
		'supplier_count': Supplier.objects.count(),
	})


@login_required
def product_list(request):
	products = Product.objects.select_related('category', 'supplier')
	query = request.GET.get('q', '').strip()
	category = request.GET.get('category', '')
	stock = request.GET.get('stock', '')
	if query:
		products = products.filter(Q(name__icontains=query) | Q(sku__icontains=query) | Q(supplier__name__icontains=query))
	if category:
		products = products.filter(category_id=category)
	if stock == 'low':
		products = products.filter(quantity__lte=F('reorder_level'))
	return render(request, 'inventory/product_list.html', {'products': products, 'categories': Category.objects.all(), 'query': query, 'selected_category': category, 'selected_stock': stock})


@login_required
def product_create(request):
	form = ProductForm(request.POST or None)
	if form.is_valid():
		form.save()
		messages.success(request, 'Product added to your inventory.')
		return redirect('product-list')
	return render(request, 'inventory/product_form.html', {'form': form, 'title': 'Add product'})


@login_required
def product_update(request, pk):
	product = get_object_or_404(Product, pk=pk)
	form = ProductForm(request.POST or None, instance=product)
	if form.is_valid():
		form.save()
		messages.success(request, 'Product details updated.')
		return redirect('product-list')
	return render(request, 'inventory/product_form.html', {'form': form, 'title': 'Edit product', 'product': product})


@login_required
def product_delete(request, pk):
	product = get_object_or_404(Product, pk=pk)
	if request.method == 'POST':
		product.delete()
		messages.success(request, 'Product removed.')
		return redirect('product-list')
	return render(request, 'inventory/confirm_delete.html', {'object': product, 'type': 'product'})


@login_required
def category_list(request):
	return render(request, 'inventory/category_list.html', {'categories': Category.objects.annotate(product_count=Count('products'))})


@login_required
def category_create(request):
	form = CategoryForm(request.POST or None)
	if form.is_valid():
		form.save()
		return redirect('category-list')
	return render(request, 'inventory/simple_form.html', {'form': form, 'title': 'Add category', 'back_url': 'category-list'})


@login_required
def category_delete(request, pk):
	category = get_object_or_404(Category, pk=pk)
	if request.method == 'POST':
		category.delete()
		return redirect('category-list')
	return render(request, 'inventory/confirm_delete.html', {'object': category, 'type': 'category'})


@login_required
def supplier_list(request):
	return render(request, 'inventory/supplier_list.html', {'suppliers': Supplier.objects.annotate(product_count=Count('products'))})


@login_required
def supplier_create(request):
	form = SupplierForm(request.POST or None)
	if form.is_valid():
		form.save()
		return redirect('supplier-list')
	return render(request, 'inventory/simple_form.html', {'form': form, 'title': 'Add supplier', 'back_url': 'supplier-list'})


@login_required
def supplier_delete(request, pk):
	supplier = get_object_or_404(Supplier, pk=pk)
	if request.method == 'POST':
		supplier.delete()
		return redirect('supplier-list')
	return render(request, 'inventory/confirm_delete.html', {'object': supplier, 'type': 'supplier'})


@login_required
def stock_operation(request):
	form = StockTransactionForm(request.POST or None)
	if form.is_valid():
		stock_transaction = form.save(commit=False)
		with transaction.atomic():
			product = Product.objects.select_for_update().get(pk=stock_transaction.product_id)
			if stock_transaction.transaction_type == StockTransaction.OUT and stock_transaction.quantity > product.quantity:
				form.add_error('quantity', 'Not enough stock available for this operation.')
			else:
				delta = stock_transaction.quantity if stock_transaction.transaction_type == StockTransaction.IN else -stock_transaction.quantity
				product.quantity = F('quantity') + delta
				product.save(update_fields=['quantity', 'updated_at'])
				stock_transaction.created_by = request.user
				stock_transaction.save()
		if not form.errors:
			messages.success(request, 'Stock operation recorded.')
			return redirect('transaction-history')
	return render(request, 'inventory/stock_operation.html', {'form': form})


@login_required
def transaction_history(request):
	transactions = StockTransaction.objects.select_related('product', 'created_by')
	transaction_type = request.GET.get('type', '')
	if transaction_type in {StockTransaction.IN, StockTransaction.OUT}:
		transactions = transactions.filter(transaction_type=transaction_type)
	return render(request, 'inventory/transaction_history.html', {'transactions': transactions, 'selected_type': transaction_type})


@login_required
def reports(request):
	categories = Category.objects.annotate(
		product_count=Count('products'),
		units=Sum('products__quantity'),
		value=Sum(ExpressionWrapper(F('products__quantity') * F('products__unit_price'), output_field=DecimalField(max_digits=14, decimal_places=2))),
	)
	transaction_totals = StockTransaction.objects.values('transaction_type').annotate(total=Sum('quantity'))
	return render(request, 'inventory/reports.html', {'categories': categories, 'transaction_totals': transaction_totals})
from django.shortcuts import render

# Create your views here.
