namespace Api.Config.Error;

// 모든 커스텀 비즈니스 예외의 기본 클래스
public abstract class BusinessException(string message) : Exception(message)
{
}

// 400: 잘못된 요청
public class BadRequestException(string message) : BusinessException(message)
{
}

// 401: 인증 실패 (보통 상속보다는 직접 사용)
public class UnauthorizedException(string message) : BusinessException(message)
{
}

// 403: 권한 부족
public class ForbiddenException(string message) : BusinessException(message)
{
}

// 404: 리소스 없음
public class NotFoundException(string message) : BusinessException(message)
{
}

// 409: 리소스 충돌 (중복 데이터 등)
public class ConflictException(string message) : BusinessException(message)
{
}
