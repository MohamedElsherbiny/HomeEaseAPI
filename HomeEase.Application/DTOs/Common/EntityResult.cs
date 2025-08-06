namespace HomeEase.Application.DTOs.Common
{
    public class EntityResult
    {
        private readonly List<EntityError> _entityErrors;
        public bool Succeeded { get; }
        public dynamic Data { get; }
        public ICollection<EntityError> WarningErrors { get; set; }
        public IReadOnlyCollection<EntityError> ValidationErrors => _entityErrors != null ? _entityErrors.AsReadOnly() : new List<EntityError>().AsReadOnly();

        private EntityResult(IEnumerable<EntityError> entityErrors)
        {
            var enumerable = entityErrors as EntityError[] ?? entityErrors.ToArray();
            if (entityErrors == null || !enumerable.Any())
            {
                throw new ArgumentNullException(nameof(entityErrors));
            }

            _entityErrors = enumerable.ToList();
        }

        private EntityResult(params EntityError[] entityErrors)
        {
            if (entityErrors == null || !entityErrors.Any())
            {
                throw new ArgumentNullException(nameof(entityErrors));
            }

            _entityErrors = new List<EntityError>(entityErrors);
        }

        private EntityResult(bool success)
        {
            Succeeded = success;
        }

        private EntityResult(bool success, dynamic data)
        {
            Succeeded = success;
            Data = data;
        }

        #region Static Methods

        public static EntityResult Success => new EntityResult(true);

        public static EntityResult SuccessWithData(dynamic data) => new EntityResult(true, data);

        public static EntityResult Failed(string message)
        {
            return new EntityResult(new EntityError(message));
        }

        public static EntityResult Failed(IEnumerable<string> messages)
        {
            return new EntityResult(messages.Select(c => new EntityError(c)));
        }

        public static EntityResult Failed(params EntityError[] errors)
        {
            return new EntityResult(errors);
        }

        public static EntityResult Failed(IEnumerable<EntityError> errors)
        {
            return new EntityResult(errors);
        }

        #endregion Static Methods
    }
}
