using System.Linq;
using WpfApp1.Models;
using WpfApp1.ViewModels;

namespace WpfApp1.Services
{
    public class AuthoService
    {
        private readonly BeverageFactoryEntities _context;

        public AuthoService(BeverageFactoryEntities context)
        {
            _context = context;
        }

        public int? SyncAutho(EmployeeViewModel vm)
        {
            if (!vm.HasSystemAccess)
            {
                if (vm.AuthoId.HasValue)
                    DeleteAutho(vm.AuthoId.Value);

                return null;
            }

            if (vm.AuthoId.HasValue)
            {
                var autho = _context.Authoes.Find(vm.AuthoId.Value);
                if (autho != null)
                {
                    autho.login = vm.Login;
                    if (!string.IsNullOrWhiteSpace(vm.Password))
                        autho.password = Hash.ComputeSha256Hash(vm.Password);

                    return autho.id;
                }
            }

            return CreateAutho(vm);
        }

        public int CreateAutho(EmployeeViewModel vm)
        {
            if (string.IsNullOrWhiteSpace(vm.Password))
                throw new System.Exception("Пароль не может быть пустым.");

            var newAutho = new Autho
            {
                login = vm.Login,
                password = Hash.ComputeSha256Hash(vm.Password)
            };

            _context.Authoes.Add(newAutho);
            _context.SaveChanges();

            return newAutho.id;
        }

        public void DeleteAutho(int id)
        {
            var autho = _context.Authoes.Find(id);
            if (autho != null)
                _context.Authoes.Remove(autho);
        }
    }
}