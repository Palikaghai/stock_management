from django.conf import settings
from django.db import models
from django.db.models import F, Q


class Category(models.Model):
	name = models.CharField(max_length=100, unique=True)
	description = models.TextField(blank=True)
	created_at = models.DateTimeField(auto_now_add=True)

	class Meta:
		ordering = ['name']
		verbose_name_plural = 'categories'

	def __str__(self):
		return self.name


class Supplier(models.Model):
	name = models.CharField(max_length=150)
	contact_person = models.CharField(max_length=120, blank=True)
	email = models.EmailField(blank=True)
	phone = models.CharField(max_length=30, blank=True)
	address = models.TextField(blank=True)

	class Meta:
		ordering = ['name']

	def __str__(self):
		return self.name


class Product(models.Model):
	sku = models.CharField(max_length=40, unique=True)
	name = models.CharField(max_length=160)
	category = models.ForeignKey(Category, on_delete=models.PROTECT, related_name='products')
	supplier = models.ForeignKey(Supplier, on_delete=models.PROTECT, related_name='products')
	description = models.TextField(blank=True)
	unit_price = models.DecimalField(max_digits=12, decimal_places=2)
	quantity = models.PositiveIntegerField(default=0)
	reorder_level = models.PositiveIntegerField(default=10)
	created_at = models.DateTimeField(auto_now_add=True)
	updated_at = models.DateTimeField(auto_now=True)

	class Meta:
		ordering = ['name']
		constraints = [
			models.CheckConstraint(condition=Q(unit_price__gte=0), name='price_non_negative'),
		]

	def __str__(self):
		return f'{self.name} ({self.sku})'

	@property
	def stock_value(self):
		return self.unit_price * self.quantity

	@property
	def is_low_stock(self):
		return self.quantity <= self.reorder_level


class StockTransaction(models.Model):
	IN = 'IN'
	OUT = 'OUT'
	TRANSACTION_TYPES = [(IN, 'Stock in'), (OUT, 'Stock out')]

	product = models.ForeignKey(Product, on_delete=models.PROTECT, related_name='transactions')
	transaction_type = models.CharField(max_length=3, choices=TRANSACTION_TYPES)
	quantity = models.PositiveIntegerField()
	note = models.CharField(max_length=255, blank=True)
	created_by = models.ForeignKey(settings.AUTH_USER_MODEL, on_delete=models.SET_NULL, null=True, blank=True)
	created_at = models.DateTimeField(auto_now_add=True)

	class Meta:
		ordering = ['-created_at']

	def __str__(self):
		return f'{self.get_transaction_type_display()} - {self.product.name}'
