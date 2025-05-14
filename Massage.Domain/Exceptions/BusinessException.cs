namespace Massage.Domain.Exceptions;

public class BusinessException(string message) : Exception(message)
{
}
