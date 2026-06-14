using System.Linq;
using WpfApp1.Models;
using WpfApp1.ViewModels;

namespace WpfApp1.Services
{
    /// <summary>
    /// Сервис для управления учетными записями (Autho) сотрудников.
    /// Выполняет создание, обновление и удаление записей авторизации.
    /// </summary>
    public class AuthoService
    {
        /// <summary>
        /// Контекст базы данных, через который выполняются операции.
        /// </summary>
        private readonly BeverageFactoryEntities _context;

        /// <summary>
        /// Конструктор, принимающий контекст базы данных.
        /// </summary>
        public AuthoService(BeverageFactoryEntities context)
        {
            _context = context;
        }

        /// <summary>
        /// Синхронизирует данные учетной записи сотрудника.
        /// Если у сотрудника есть доступ к системе — создаёт или обновляет Autho.
        /// Если доступа нет — удаляет Autho.
        /// </summary>
        /// <param name="vm">Модель сотрудника.</param>
        /// <returns>ID записи Autho или null, если запись удалена.</returns>
        public int? SyncAutho(EmployeeViewModel vm)
        {
            // Если сотрудник НЕ должен иметь доступ к системе
            if (!vm.HasSystemAccess)
            {
                // Если у него была запись Autho — удаляем её
                if (vm.AuthoId.HasValue)
                    DeleteAutho(vm.AuthoId.Value);

                return null;
            }

            // Если запись Autho уже существует — обновляем её
            if (vm.AuthoId.HasValue)
            {
                var autho = _context.Authoes.Find(vm.AuthoId.Value);
                if (autho != null)
                {
                    // Обновляем логин
                    autho.login = vm.Login;

                    // Если пароль введён — обновляем и его
                    if (!string.IsNullOrWhiteSpace(vm.Password))
                        autho.password = Hash.ComputeSha256Hash(vm.Password);

                    return autho.id;
                }
            }

            // Если записи нет — создаём новую
            return CreateAutho(vm);
        }

        /// <summary>
        /// Создаёт новую запись Autho для сотрудника.
        /// </summary>
        /// <param name="vm">Модель сотрудника.</param>
        /// <returns>ID созданной записи Autho.</returns>
        /// <exception cref="System.Exception">Выбрасывается, если пароль пустой.</exception>
        public int CreateAutho(EmployeeViewModel vm)
        {
            // Проверяем, что пароль указан
            if (string.IsNullOrWhiteSpace(vm.Password))
                throw new System.Exception("Пароль не может быть пустым.");

            // Создаём новую запись
            var newAutho = new Autho
            {
                login = vm.Login,
                password = Hash.ComputeSha256Hash(vm.Password) // Хэшируем пароль
            };

            // Добавляем в базу
            _context.Authoes.Add(newAutho);
            _context.SaveChanges();

            return newAutho.id;
        }

        /// <summary>
        /// Удаляет запись Autho по ID.
        /// </summary>
        /// <param name="id">ID записи Autho.</param>
        public void DeleteAutho(int id)
        {
            var autho = _context.Authoes.Find(id);

            /* 
             * Если запись найдена — удаляем.
             * SaveChanges() вызывается в вызывающем коде,
             * чтобы можно было удалять несколько записей за раз.
             */
            if (autho != null)
                _context.Authoes.Remove(autho);
        }
    }
}
