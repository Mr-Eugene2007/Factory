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
    public class EmployeeService : IDisposable
    {
        private readonly BeverageFactoryEntities _context;
        private readonly AuthoService _authoService;

        public EmployeeService()
        {
            _context = new BeverageFactoryEntities();
            _authoService = new AuthoService(_context);
        }

        public object LoadOriginalEntity(string role, int id)
        {
            return role == "Поставщик"
                ? (object)_context.Suppliers.FirstOrDefault(s => s.id == id)
                : _context.Customers.FirstOrDefault(c => c.id == id);
        }

        public void CreateEmployee(EmployeeViewModel vm)
        {
            ValidateEmployee(vm);

            var entity = vm.IsSupplier
                ? (object)new Supplier()
                : new Customer();

            UpdateEntityFields(entity, vm);

            var authoId = _authoService.SyncAutho(vm);
            SetAuthoId(entity, authoId);

            AddEntity(entity);
            _context.SaveChanges();
        }

        public void UpdateEmployee(EmployeeViewModel vm)
        {
            ValidateEmployee(vm);

            if (vm.OriginalEntity == null)
                return;

            UpdateEntityFields(vm.OriginalEntity, vm);

            var authoId = _authoService.SyncAutho(vm);
            SetAuthoId(vm.OriginalEntity, authoId);

            _context.SaveChanges();
        }

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

        private void UpdateEntityFields(object entity, EmployeeViewModel vm)
        {
            dynamic e = entity;
            e.last_name = vm.LastName;
            e.name = vm.Name;
            e.surname = vm.Surname;
            e.phone = vm.Phone;
            e.email = vm.Email;
        }

        private void SetAuthoId(object entity, int? id)
        {
            dynamic e = entity;
            e.autho_id = id;
        }

        private void AddEntity(object entity)
        {
            if (entity is Supplier s)
                _context.Suppliers.Add(s);
            else if (entity is Customer c)
                _context.Customers.Add(c);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }

        private void ValidateEmployee(EmployeeViewModel vm)
        {
            List<ValidationResult> results;

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
            else
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

            if (results.Count > 0)
            {
                string errors = string.Join("\n", results.Select(r => r.ErrorMessage));
                throw new ValidationException(errors);
            }
        }

    }
}