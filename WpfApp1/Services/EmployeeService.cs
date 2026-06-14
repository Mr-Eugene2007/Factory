using System;
using System.Linq;
using WpfApp1.Models;
using WpfApp1.ViewModels;
using WpfApp1.Validators;
using WpfApp1.Models.Validation;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace WpfApp1.Services
{
    /// <summary>
    /// Сервис для управления сотрудниками (Поставщики и Клиенты).
    /// Выполняет создание, обновление, удаление и валидацию данных.
    /// Также синхронизирует учетные записи Autho через AuthoService.
    /// </summary>
    public class EmployeeService : IDisposable
    {
        /// <summary>
        /// Контекст базы данных.
        /// </summary>
        private readonly BeverageFactoryEntities _context;

        /// <summary>
        /// Сервис для работы с учетными записями авторизации.
        /// </summary>
        private readonly AuthoService _authoService;

        /// <summary>
        /// Конструктор. Создаёт контекст и сервис авторизации.
        /// </summary>
        public EmployeeService()
        {
            _context = new BeverageFactoryEntities();
            _authoService = new AuthoService(_context);
        }

        /// <summary>
        /// Загружает исходную сущность сотрудника по роли и ID.
        /// Используется при редактировании.
        /// </summary>
        public object LoadOriginalEntity(string role, int id)
        {
            return role == "Поставщик"
                ? (object)_context.Suppliers.FirstOrDefault(s => s.id == id)
                : _context.Customers.FirstOrDefault(c => c.id == id);
        }

        /// <summary>
        /// Создаёт нового сотрудника (Поставщика или Клиента).
        /// </summary>
        /// <param name="vm">Модель сотрудника.</param>
        public void CreateEmployee(EmployeeViewModel vm)
        {
            // Проверяем корректность данных
            ValidateEmployee(vm);

            // Создаём сущность нужного типа
            var entity = vm.IsSupplier
                ? (object)new Supplier()
                : new Customer();

            // Заполняем поля сущности
            UpdateEntityFields(entity, vm);

            // Создаём или обновляем Autho
            var authoId = _authoService.SyncAutho(vm);
            SetAuthoId(entity, authoId);

            // Добавляем в базу
            AddEntity(entity);
            _context.SaveChanges();
        }

        /// <summary>
        /// Обновляет данные существующего сотрудника.
        /// </summary>
        public void UpdateEmployee(EmployeeViewModel vm)
        {
            ValidateEmployee(vm);

            if (vm.OriginalEntity == null)
                return;

            // Обновляем поля
            UpdateEntityFields(vm.OriginalEntity, vm);

            // Синхронизируем Autho
            var authoId = _authoService.SyncAutho(vm);
            SetAuthoId(vm.OriginalEntity, authoId);

            _context.SaveChanges();
        }

        /// <summary>
        /// Удаляет сотрудника и связанную запись Autho.
        /// </summary>
        public void DeleteEmployee(EmployeeViewModel vm)
        {
            if (vm.OriginalEntity == null)
                throw new Exception("Исходная запись сотрудника не найдена.");

            dynamic e = vm.OriginalEntity;

            // 1. Удаляем Autho, если есть
            if (e.autho_id != null)
            {
                var autho = _context.Authoes.Find((int)e.autho_id);
                if (autho != null)
                {
                    _context.Authoes.Remove(autho);
                    _context.SaveChanges(); // важно!
                }
            }

            // 2. Удаляем сотрудника
            if (vm.IsSupplier)
                _context.Suppliers.Remove((Supplier)e);
            else
                _context.Customers.Remove((Customer)e);

            _context.SaveChanges();
        }

        /// <summary>
        /// Обновляет поля сущности сотрудника из ViewModel.
        /// </summary>
        private void UpdateEntityFields(object entity, EmployeeViewModel vm)
        {
            dynamic e = entity;

            /* 
             * Заполняем основные поля сотрудника.
             * Используем dynamic, чтобы одинаково работать
             * и с Supplier, и с Customer.
             */
            e.last_name = vm.LastName;
            e.name = vm.Name;
            e.surname = vm.Surname;
            e.phone = vm.Phone;
            e.email = vm.Email;
        }

        /// <summary>
        /// Устанавливает autho_id для сущности.
        /// </summary>
        private void SetAuthoId(object entity, int? id)
        {
            dynamic e = entity;
            e.autho_id = id;
        }

        /// <summary>
        /// Добавляет сущность в нужную таблицу.
        /// </summary>
        private void AddEntity(object entity)
        {
            if (entity is Supplier s)
                _context.Suppliers.Add(s);
            else if (entity is Customer c)
                _context.Customers.Add(c);
        }

        /// <summary>
        /// Освобождает ресурсы контекста.
        /// </summary>
        public void Dispose()
        {
            _context?.Dispose();
        }

        /// <summary>
        /// Выполняет валидацию данных сотрудника.
        /// Использует разные модели и валидаторы для Клиента и Поставщика.
        /// </summary>
        private void ValidateEmployee(EmployeeViewModel vm)
        {
            List<ValidationResult> results;

            // Если клиент
            if (vm.IsCustomer)
            {
                var model = new CustomerValidationModel
                {
                    name = vm.Name,
                    last_name = vm.LastName,
                    surname = vm.Surname,
                    phone = vm.Phone,
                    email = vm.Email
                };

                results = new CustomerValidator().Validate(model);
            }
            else // Если поставщик
            {
                var model = new SupplierValidationModel
                {
                    name = vm.Name,
                    last_name = vm.LastName,
                    surname = vm.Surname,
                    phone = vm.Phone,
                    email = vm.Email
                };

                results = new SupplierValidator().Validate(model);
            }

            // Если есть ошибки — выбрасываем исключение
            if (results.Count > 0)
            {
                string errors = string.Join("\n", results.Select(r => r.ErrorMessage));
                throw new ValidationException(errors);
            }
        }
    }
}
