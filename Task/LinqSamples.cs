// Copyright © Microsoft Corporation.  All Rights Reserved.
// This code released under the terms of the 
// Microsoft Public License (MS-PL, http://opensource.org/licenses/ms-pl.html.)
//
//Copyright (C) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using SampleSupport;
using Task.Data;
using System.Text.RegularExpressions;

// Version Mad01

namespace SampleQueries
{
    [Title("LINQ Module")]
    [Prefix("Linq")]
    public class LinqSamples : SampleHarness
    {
        private DataSource dataSource = new DataSource();

        [Category("Restriction Operators")]
        [Title("Linq 01")]
        [Description("Выдайте список всех клиентов, чей суммарный оборот (сумма всех заказов) превосходит некоторую величину X.")]
        public void Linq01()
        {
            foreach (var customer in dataSource.Customers)
            {
                // Вывод всех клиентов с суммой всех их заказов.
                ObjectDumper.Write(String.Format("Customer ID: {0}\nAll orders total: {1} ", customer.CustomerID, customer.Orders.Sum(o => o.Total)));
                // Вывод каждого заказа.
                foreach (var order in customer.Orders)
                {
                    ObjectDumper.Write(String.Format("Order ID: {0}\nTotal: {1}", order.OrderID, order.Total));
                }
                ObjectDumper.Write("\n");
            }


            var filteredCustomers = dataSource.Customers.Select(c => new { AllOrdersTotal = c.Orders.Sum(o => o.Total), CustomeriD = c.CustomerID }).Where(t => t.AllOrdersTotal < 1000);
            ObjectDumper.Write("\nResult:\n");
            foreach (var customer in filteredCustomers)
            {
                ObjectDumper.Write(String.Format("Customer ID: {0}\nAll orders total: {1} ", customer.CustomeriD, customer.AllOrdersTotal));
            }
        }

        [Category("Restriction Operators")]
        [Title("Linq 02")]
        [Description("Для каждого клиента составьте список поставщиков, находящихся в той же стране и том же городе. " +
                     "Сделайте задания с использованием группировки и без.")]
        public void Linq02()
        {
            // Вывод всех клиентов.
            foreach (Customer customer in dataSource.Customers)
            {
                ObjectDumper.Write(String.Format("CustomerId: {0}\nCustomerCountry: {1}\nCustomerCity: {2}", customer.CustomerID, customer.Country, customer.City));
            }

            // Вывод всех поставщиков.
            foreach (Supplier supplier in dataSource.Suppliers)
            {
                ObjectDumper.Write(String.Format("SupilerName: {0}\nSupplierCountry: {1}\nSupplierCity: {2}", supplier.SupplierName, supplier.Country, supplier.City));
            }

            // Поставщики и клиенты, находящиеся в одинаковых странах и городах.
            var ClientsSuppliers = dataSource.Customers.
                Join(dataSource.Suppliers,
                     x => new { City = x.City, Country = x.Country },
                     y => new { City = y.City, Country = y.Country },
                     (c, s) => new
                     {
                         CustomerId = c.CustomerID,
                         SupplierName = s.SupplierName,
                         Country = c.Country,
                         City = c.City
                     }).GroupBy(x => new { Country = x.Country, City = x.City });

            ObjectDumper.Write("\nResult:\n");

            foreach (var clientSuppliergroup in ClientsSuppliers)
            {
                ObjectDumper.Write(String.Format("Country: {0}\nCity: {1}", clientSuppliergroup.Key.Country, clientSuppliergroup.Key.City));

                foreach (var item in clientSuppliergroup)
                {
                    ObjectDumper.Write(String.Format("CustomerId: {0}\nSupplier name: {1}", item.CustomerId, item.SupplierName));
                }
            }

        }

        [Category("Restriction Operators")]
        [Title("Linq 03")]
        [Description("Найдите всех клиентов, у которых были заказы, превосходящие по сумме величину X")]
        public void Linq03()
        {
            // Вывод всех заказов каждого клиента.
            foreach (Customer customer in dataSource.Customers)
            {
                ObjectDumper.Write(String.Format("Customer ID: {0}", customer.CustomerID));

                foreach (Order order in customer.Orders)
                {
                    ObjectDumper.Write(String.Format("Order ID: {0}\nTotal: {1}", order.OrderID, order.Total));
                }
            }

            var filteredCustomers = dataSource.Customers.Where(c => c.Orders.Any(o => o.Total > 10000));

            ObjectDumper.Write("\nResult:\n");

            foreach (Customer customer in filteredCustomers)
            {
                ObjectDumper.Write(String.Format("Customer ID: {0}", customer.CustomerID));
            }
        }

        [Category("Restriction Operators")]
        [Title("Linq 04")]
        [Description("Выдайте список клиентов с указанием, начиная с какого месяца какого года они стали клиентами (принять за таковые месяц и год самого первого заказа)")]
        public void Linq04()
        {
            // Вывод всех заказов каждого клиента.
            foreach (Customer customer in dataSource.Customers)
            {
                ObjectDumper.Write(String.Format("Customer ID: {0}", customer.CustomerID));

                foreach (Order order in customer.Orders)
                {
                    ObjectDumper.Write(String.Format("Order ID: {0}\nOrder date: {1}", order.OrderID, order.OrderDate));
                }
            }

            
            var filteredCustomers = dataSource.Customers.Select(
                c =>
                {
                    // Nullable - есть пользователи, которые ни разу не совершали заказ.
                    DateTime? minOrderDate = c.Orders.Count() != 0 ? c.Orders.Min(x => x.OrderDate) : (DateTime?)null;

                    return new
                    {
                        CustomerId = c.CustomerID,
                        FirstOrderYear = minOrderDate.HasValue ? minOrderDate.Value.Year : (int?)null,
                        FirstOrderMonth = minOrderDate.HasValue ? minOrderDate.Value.Month : (int?)null,
                    };
                });

            ObjectDumper.Write("\nResult:\n");

            foreach (var item in filteredCustomers)
            {
                ObjectDumper.Write(String.Format("Filtered Customer ID: {0}\nFirstOrderYear: {1}\nFirstOrderMonth{2}", item.CustomerId, item.FirstOrderYear, item.FirstOrderMonth));
            }

        }

        [Category("Restriction Operators")]
        [Title("Linq 05")]
        [Description("Сделайте предыдущее задание, но выдайте список отсортированным по году, месяцу, оборотам клиента (от максимального к минимальному) и имени клиента")]
        public void Linq05()
        {
            // Вывод всех заказов каждого клиента.
            foreach (Customer customer in dataSource.Customers)
            {
                ObjectDumper.Write(String.Format("Customer ID: {0}, All orders total {1}", customer.CustomerID, customer.Orders.Sum(o => o.Total)));

                foreach (Order order in customer.Orders)
                {
                    ObjectDumper.Write(String.Format("Order ID: {0}\nOrder date: {1}\nOrder total", order.OrderID, order.OrderDate, order.Total));
                }
            }

            var filteredCustomers = dataSource.Customers.Select(
                c =>
                {
                    // Nullable - есть пользователи, которые ни разу не совершали заказ.
                    DateTime? minOrderDate = c.Orders.Count() != 0 ? c.Orders.Min(x => x.OrderDate) : (DateTime?)null;

                    return new
                    {
                        CustomerId = c.CustomerID,
                        FirstOrderYear = minOrderDate.HasValue ? minOrderDate.Value.Year : (int?)null,
                        FirstOrderMonth = minOrderDate.HasValue ? minOrderDate.Value.Month : (int?)null,
                        AllOrdersTotal = c.Orders.Count() != 0? c.Orders.Sum(o => o.Total) : 0
                    };
                }).OrderByDescending(x => x.FirstOrderYear).
                   ThenByDescending(x => x.FirstOrderMonth).
                   ThenByDescending(x => x.AllOrdersTotal).
                   ThenByDescending(x => x.CustomerId);


            ObjectDumper.Write("\nResult:\n");

            foreach (var item in filteredCustomers)
            {
                ObjectDumper.Write(String.Format("FirstOrderYear: {0}\nFirstOrderMonth{1}\nAll orders total: {2}\nCustomerId: {3}", 
                                                 item.FirstOrderYear, item.FirstOrderMonth, item.AllOrdersTotal, item.CustomerId));
            }
        }

        [Category("Restriction Operators")]
        [Title("Linq 06")]
        [Description("Укажите всех клиентов, у которых указан нецифровой почтовый код или не заполнен регион или в телефоне не указан код оператора (для простоты считаем, что это равнозначно «нет круглых скобочек в начале»).")]
        public void Linq06()
        {
            foreach (Customer customer in dataSource.Customers)
            {
                ObjectDumper.Write(String.Format("CustomerID: {0}\nPostalCode: {1}\nRegion: {2}\nPhone: {3}", customer.CustomerID, customer.PostalCode,  customer.Region, customer.Phone));
            }

            var filteredClients = dataSource.Customers.Where(
                c =>
                {
                    return !Regex.IsMatch(c.PostalCode, @"^[0-9]+(-[0-9]+)+$") ||
                    String.IsNullOrEmpty(c.Region) ||
                    !c.Phone.StartsWith("(");
                });

            ObjectDumper.Write("\nResult:\n");

            foreach (Customer item in filteredClients)
            {
                ObjectDumper.Write(String.Format("Customer id: {0}", item.CustomerID));
            }
        }

        [Category("Restriction Operators")]
        [Title("Linq 07")]
        [Description("Сгруппируйте все продукты по категориям, внутри – по наличию на складе, внутри последней группы отсортируйте по стоимости.")]
        public void Linq07()
        {
            // Вывод продуктов.
            foreach (Product product in dataSource.Products)
            {
                ObjectDumper.Write(String.Format("ProductId: {0}\nCategory: {1}\nUnitsInStock: {2}\nPrice: {3}", 
                                                 product.ProductID, product.Category, product.UnitsInStock, product.UnitPrice));
            }

            var productGroups = dataSource.Products.
                GroupBy(p => p.Category, (category, products) => new
                {
                    Category = category,
                    GroupByHavingInStock = products.GroupBy(p => p.UnitsInStock > 0).
                                                    Select(x => new
                                                    {
                                                        HasInStock = x.Key,
                                                        Products = x.OrderBy(p => p.UnitPrice)
                                                    })
                });

            ObjectDumper.Write("Result");

            foreach (var byCategoryGroup in productGroups)
            {
                ObjectDumper.Write(String.Format("Category: {0}\n", byCategoryGroup.Category));
                foreach (var byHavingInStock in byCategoryGroup.GroupByHavingInStock)
                {
                    ObjectDumper.Write(String.Format("Has in stock: {0}", byHavingInStock.HasInStock));
                    foreach (var product in byHavingInStock.Products)
                    {
                        ObjectDumper.Write(String.Format("Product: {0} Price: {1}", product.ProductName, product.UnitPrice));
                    }
                }
            }
        }

        [Category("Restriction Operators")]
        [Title("Linq 08")]
        [Description("Сгруппируйте товары по группам «дешевые», «средняя цена», «дорогие». Границы каждой группы задайте сами.")]
        public void Linq08()
        {
            foreach (Product product in dataSource.Products)
            {
                ObjectDumper.Write(String.Format("ProductId: {0}\nProductName: {1}\nProductPrice: {2}", product.ProductID, product.ProductName, product.UnitPrice));
            }

            decimal boundsOfCheap = 20m;
            decimal boundsOfExpensive = 30m;

            var productGroups = dataSource.Products.GroupBy(
                p =>
                {
                    if (p.UnitPrice < boundsOfCheap)
                    {
                        return "cheap";
                    }
                    else if (p.UnitPrice > boundsOfCheap && p.UnitPrice < boundsOfExpensive)
                    {
                        return "medium";
                    }
                    else
                    {
                        return "expensive";
                    }
                });

            foreach (var group in productGroups)
            {
                ObjectDumper.Write(String.Format("PriceCategory: {0}", group.Key.ToString()));
                foreach (Product product in group)
                {
                    ObjectDumper.Write(String.Format("ProductId: {0}\nProductName: {1}\nProductPrice: {2}", product.ProductID, product.ProductName, product.UnitPrice));
                }
            }
        }

        [Category("Restriction Operators")]
        [Title("Linq 09")]
        [Description("Рассчитайте среднюю прибыльность каждого города (среднюю сумму заказа по всем клиентам из данного города) " +
                     "и среднюю интенсивность (среднее количество заказов, приходящееся на клиента из каждого города).")]
        public void Linq09()
        {
            // Группировка всех клиентов по городу.
            // Каждая группа содержит среднюю сумму заказов всех клиентов в городе.
            var averageStatisticsByCity = dataSource.Customers.GroupBy(c => c.City).Select(customers => new
            {
                City = customers.Key,
                AverageProfit = customers.Average(c => c.Orders.Sum(o => o.Total)), // Средняя сумма всех заказов, совершенные всеми клиентами определенного города.
                AverageIntencity = customers.Average(p => p.Orders.Count())         // Cреднее количество заказов, приходящееся на клиента из каждого города
            });

            foreach (var item in averageStatisticsByCity)
            {
                ObjectDumper.Write(String.Format("City: {0}, Average profit: {1}, Average intencity: {2}", item.City, item.AverageProfit, item.AverageIntencity));
            }
        }

        [Category("Restriction Operators")]
        [Title("Linq 10")]
        [Description("Сделайте среднегодовую статистику активности клиентов по месяцам (без учета года), " +
                     "статистику по годам, " +
                     "по годам и месяцам (т.е. когда один месяц в разные годы имеет своё значение).")]
        public void Linq10()
        {
            var customerStatisticsPerMonth = dataSource.Customers.SelectMany(c => c.Orders).GroupBy(o => o.OrderDate.Month, (month, orders) => new
            {
                Key = month,
                Count = orders.Count()
            });

            var customerStatisticsPerYear = dataSource.Customers.SelectMany(c => c.Orders).GroupBy(o => o.OrderDate.Year, (year, orders) => new
            {
                Key = year,
                Count = orders.Count()
            });

            var customerStatisticsPerMonthYear = dataSource.Customers.SelectMany(c => c.Orders).GroupBy(o => new { Month = o.OrderDate.Month, Year = o.OrderDate.Year }, (monthYear, orders) => new
            {
                Month = monthYear.Month,
                Year = monthYear.Year,
                Count = orders.Count()
            }).OrderBy(x => x.Year).ThenBy(x => x.Month);

            ObjectDumper.Write("Customer Statistics Per Month");
            foreach (var group in customerStatisticsPerMonth)
            {
                ObjectDumper.Write(String.Format("Month: {0} Orders: {1}", group.Key, group.Count));
            }

            ObjectDumper.Write("\n\n\n");

            ObjectDumper.Write("Customer Statistics Per Year");
            foreach (var group in customerStatisticsPerYear)
            {
                ObjectDumper.Write(String.Format("Year: {0} Orders: {1}", group.Key, group.Count));
            }

            ObjectDumper.Write("\n\n\n");

            ObjectDumper.Write("Customer Statistics Per Year and Month");
            foreach (var group in customerStatisticsPerMonthYear)
            {
                ObjectDumper.Write((String.Format("Year: {0} Month: {1} Orders: {2}", group.Year, group.Month, group.Count));
            }

        }

    }
}
