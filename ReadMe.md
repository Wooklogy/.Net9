# Entity 작성 규칙 (EF Core)

이 프로젝트의 Entity는 **DB 중심 / 명시적 선언 / 자동 문서화**를 기준으로 설계한다.
편의성보다 **추적 가능성, 유지보수성, 예측 가능성**을 우선한다.

본 문서에 정의된 규칙은 **모든 Entity에 강제 적용**된다.

---

## 1. 기본 설계 원칙

* Entity ↔ DB Table **1:1 매칭**
* DB 스키마가 최종 진실(Source of Truth)
* EF Core 암묵적 Convention 사용 금지
* 모든 컬럼은 Attribute로 **명시적으로 정의**
* Swagger / Validation / DB Comment를 Entity에서 모두 기술

---

## 2. 필수 using

모든 Entity는 아래 using을 기본 포함한다.

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

using Microsoft.EntityFrameworkCore;

using Swashbuckle.AspNetCore.Annotations;
```

---

## 3. Entity 클래스 선언 규칙

```csharp
[Table("user")]
[Index(nameof(Identify), IsUnique = true)]
[Comment("User Table")]
public class UserEntity : BaseEntityUuid
```

### 규칙

* 테이블명은 **소문자 + snake_case**
* 모든 Entity는 `BaseEntityUuid` 상속
* Index는 반드시 Attribute로 선언
* `[Comment]` 필수 (DB 문서화 목적)

---

## 4. 컬럼(Property) 작성 규칙

### 기본 형식 (표준)

```csharp
[SwaggerSchema("User login ID")]
[Column("identify")]
[Required]
[MaxLength(128)]
[MinLength(4)]
[Comment("User login ID")]
public required string Identify { get; set; }
```

### 필수 요소

| 항목                   | 필수 |
| -------------------- | -- |
| SwaggerSchema        | ✅  |
| Column               | ✅  |
| Comment              | ✅  |
| Required / AllowNull | ✅  |
| Length 제한 (string)   | ✅  |

하나라도 누락되면 잘못된 Entity로 간주한다.

---

## 5. Required / Nullable 규칙

```csharp
[Required]
public required string Identify { get; set; }

[AllowNull]
public DateTime? LastLoginAt { get; set; }
```

### 규칙

* Nullable 타입(`?`) → 반드시 `[AllowNull]`
* Non-Nullable 타입 → `[Required] + required 키워드`
* 애매한 Nullable 선언 금지

---

## 6. Enum 사용 규칙

```csharp
[SwaggerSchema("User Role")]
[Column("role")]
[Required]
[Comment("User role")]
public required EnumRole Role { get; set; } = EnumRole.User;
```

### 규칙

* Enum은 DB에 **숫자(int)** 로 저장
* string Enum 사용 금지
* 기본값 반드시 명시
* Swagger 설명 필수

---

## 7. FK / 관계 정의 규칙

```csharp
[SwaggerSchema("CreatedBy UUID")]
[Column("created_by")]
[AllowNull]
[Comment("Which admin created this user?")]
public Guid? CreatedById { get; set; }

[ForeignKey(nameof(CreatedById))]
public UserEntity? CreatedBy { get; set; }
```

### 규칙

* FK 컬럼은 **명시적 Guid**
* Navigation Property는 Nullable
* `[ForeignKey]` 필수
* Cascade / Delete Rule은 **Fluent API에서만 정의**

---

## 8. 금지 사항 ❌

아래 패턴은 전부 금지한다.

```csharp
public string Name { get; set; }          // ❌ Column 미정의
public string? Email { get; set; }        // ❌ AllowNull 없음
public int Age;                           // ❌ Property 아님
public virtual Role Role { get; set; }    // ❌ Lazy Loading 유도
```

* 암묵적 Convention 의존
* virtual Navigation Property
* 설명 없는 컬럼
* 의미 없는 Nullable

---

## 9. 예시: UserEntity (표준 구현)

```csharp
[Table("user")]
[Index(nameof(Identify), IsUnique = true)]
[Comment("User Table")]
public class UserEntity : BaseEntityUuid
{
    [SwaggerSchema("User login ID")]
    [Column("identify")]
    [Required]
    [MaxLength(128)]
    [MinLength(4)]
    [Comment("User login ID")]
    public required string Identify { get; set; }

    [SwaggerSchema("User Login Password")]
    [Column("password")]
    [Required]
    [MaxLength(128)]
    [MinLength(4)]
    [Comment("User login Password as Hashing key")]
    public required string Password { get; set; }

    [SwaggerSchema("User Role")]
    [Column("role")]
    [Required]
    [Comment("User role")]
    public required EnumRole Role { get; set; } = EnumRole.User;

    [SwaggerSchema("Last Login Date")]
    [Column("last_login_at")]
    [AllowNull]
    [Comment("User latest login time")]
    public DateTime? LastLoginAt { get; set; }

    [SwaggerSchema("CreatedBy UUID")]
    [Column("created_by")]
    [AllowNull]
    [Comment("Which admin created this user?")]
    public Guid? CreatedById { get; set; }

    [ForeignKey(nameof(CreatedById))]
    public UserEntity? CreatedBy { get; set; }
}
```


# Service 작성 규칙 (Application Layer)

이 프로젝트의 Service는 **비즈니스 로직의 단일 진입점**이다.
Controller는 요청을 전달만 하며, **모든 판단·처리는 Service에서 수행**한다.

본 문서에 정의된 규칙은 **모든 Service에 강제 적용**된다.

---

## 1. 기본 설계 원칙

* Service는 **Application Layer**에 위치
* Controller는 절대 비즈니스 로직을 갖지 않는다
* DB 접근은 Service에서만 허용
* 트랜잭션 단위는 Service 기준
* try-catch 사용 금지 (Middleware에서 전역 처리)

---

## 2. Service 기본 구조

```csharp
using Microsoft.EntityFrameworkCore;

public class AuthService
{
    private readonly AppDbContext _db;

    public AuthService(AppDbContext db)
    {
        _db = db;
    }
}
```

### 규칙

* Service 이름은 `XXXService`
* Interface 분리는 **필요한 시점에만**
* 의존성은 생성자 주입만 사용

---

## 3. Exception 처리 규칙

Service에서는 `try-catch`를 **절대 사용하지 않는다.**
Exception은 그대로 throw 하고 전역 Middleware에서 처리한다.

### 허용된 Exception 타입만 사용한다

* `BadRequestException`
* `ConflictException`
* `ForbiddenException`
* `NotFoundException`
* `UnauthorizedException`
* `ValidationException`

직접 Exception을 새로 만들거나 `System.Exception` 사용을 금지한다.

---

## 4. QTO / STO 규칙

### 용어 정의

* **QTO**: reQstued Transfer Object  (요청 모델)
* **STO**: reSponsed Transfer Object (응답 모델)

Service 메서드는 **반드시 QTO를 입력**으로 받고,
**Primitive 타입 또는 STO를 반환**한다.

Entity는 외부로 절대 반환하지 않는다.

---

## 5. QTO 작성 규칙

* 요청 데이터 정의 전용
* Validation Attribute 필수
* 비즈니스 로직 포함 금지

### 예시

```csharp
using System.ComponentModel.DataAnnotations;

public sealed class PostSignUpQTO
{
    [Required]
    [RegularExpression(
        @"^[a-zA-Z0-9]{4,20}$",
        ErrorMessage = "validation_identify"
    )]
    public required string Identify { get; set; }

    [Required]
    [RegularExpression(
        @"^(?=.*[A-Za-z])(?=.*\\d)(?=.*[!@#$%^&*()_\\-+=\\[\\]{};:'\"",.<>/?\\\\|~`])[A-Za-z\\d!@#$%^&*()_\\-+=\\[\\]{};:'\"",.<>/?\\\\|~`]{8,32}$",
        ErrorMessage = "validation_password"
    )]
    public required string Password { get; set; }
}
```

```csharp
public sealed class GetLoginQTO
{
    [Required]
    public required string Identify { get; set; }

    [Required]
    public required string Password { get; set; }
}
```

```csharp
public sealed class GetAllUsersQTO : BasePaginationQTO
{
    public string? keyword { get; set; }
}
```

---

## 6. STO 작성 규칙

* 응답 데이터 전용
* Validation Attribute 불필요
* Entity 구조를 그대로 노출하지 않는다

### 예시

```csharp
public sealed class GetAllUserSTO
{
    public required Guid Uuid { get; set; }
    public required string Identify { get; set; }
    public EnumRole? Role { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public required DateTime CreatedAt { get; set; }
}
```

---

## 7. Service 구현 예시

```csharp
public class AuthService
{
    private readonly AppDbContext _db;
    private readonly JwtTokenService _jwt;

    public AuthService(AppDbContext db, JwtTokenService jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    public async Task SignUp(PostSignUpQTO qto)
    {
        var exists = await _db.Users
            .AnyAsync(x => x.Identify == qto.Identify);

        if (exists)
            throw new ConflictException("exception_duplicate_user_id");

        var user = new UserEntity
        {
            Identify = qto.Identify,
            Password = ToolHash.Bcrypt(qto.Password),
            Role = EnumRole.User
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
    }

    public async Task<string> Login(GetLoginQTO qto)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(x => x.Identify == qto.Identify)
            ?? throw new BadRequestException("exception_notfound_user_infomation");

        if (!ToolHash.Verify(qto.Password, user.Password))
            throw new BadRequestException("exception_notfound_user_infomation");

        user.LastLoginAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return _jwt.GenerateToken(user.Uuid, user.Identify);
    }

    public async Task<BasePaginationSTO<GetAllUserSTO>> GetAllUser(GetAllUsersQTO qto)
    {
        IQueryable<UserEntity> query = _db.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(qto.keyword))
        {
            var keyword = qto.keyword.Trim();
            query = query.Where(u => u.Identify.Contains(keyword));
        }

        var totalCount = await query.CountAsync();

        var contents = await query
            .OrderBy(u => u.CreatedAt)
            .Skip(qto.Skip)
            .Take(qto.SafeSize)
            .Select(u => new GetAllUserSTO
            {
                Uuid = u.Uuid,
                Identify = u.Identify,
                Role = u.Role,
                LastLoginAt = u.LastLoginAt,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();

        return new BasePaginationSTO<GetAllUserSTO>(
            qto.SafePage,
            qto.SafeSize,
            totalCount,
            contents
        );
    }
}
```

---

## 8. Pagination 규칙

* 페이징 요청은 `BasePaginationQTO` 상속
* 반환은 `BasePaginationSTO<T>` 사용
* Service 내부에서 `Skip / Take` 처리

---

## 9. 삭제(Soft / Hard) 규칙

### 기본 삭제 정책: Soft Delete

```csharp
_db.Users.Remove(entity);
```

### Hard Delete 필요 시

```csharp
_db.Users.HardRemove(entity);
```

### 복구가 필요한 경우

```csharp
_db.Users.Restore(entity);
```

삭제 정책은 **Service에서만 결정**한다.

---

## 10. 목적

* Controller 단순화
* 비즈니스 규칙 중앙 집중
* 테스트 용이성 확보
* 예외 처리 일관성 유지
* 장기 운영 기준 확립


# Controller 작성 규칙 (Presentation Layer)

이 프로젝트의 Controller는 **HTTP 진입점** 역할만 수행한다.
비즈니스 로직, 권한 판단, 데이터 처리 책임은 **절대 Controller에 두지 않는다**.

본 문서에 정의된 규칙은 **모든 Controller에 강제 적용**된다.

---

## 1. 기본 설계 원칙

* Controller는 **요청 수신 / 응답 반환만 담당**
* 모든 비즈니스 로직은 Service에 위임
* try-catch 사용 금지 (전역 Middleware에서 예외 처리)
* Entity 직접 사용 금지 (QTO / STO만 사용)
* Swagger 문서 완성 책임은 Controller에 있음

---

## 2. 기본 Controller 구조

```csharp
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public AuthController(
        AuthService authService,
        ILogger<ExceptionHandlingMiddleware> logger
    )
    {
        _authService = authService;
        _logger = logger;
    }
}
```

### 규칙

* Route는 **리소스 기준**으로 정의
* `ControllerBase`만 상속 (`Controller` 금지)
* DI는 생성자 주입만 허용

---

## 3. Action Method 규칙

```csharp
[HttpPost("sign_up")]
[ProducesResponseType(StatusCodes.Status201Created)]
[ProducesErrorCodes(StatusCodes.Status400BadRequest, StatusCodes.Status409Conflict)]
[SwaggerOperation(Summary = "Create User", Description = "Sign up user account")]
public async Task<IActionResult> SignUp(PostSignUpQTO qto)
{
    await _authService.SignUp(qto);
    return StatusCode(StatusCodes.Status201Created);
}
```

### 규칙

* Action은 반드시 **비동기(async)**
* Request/Response 타입은 명시적으로 선언
* HTTP StatusCode를 명확히 반환
* Controller에서 데이터 가공 금지

---

## 4. Swagger / Response Annotation 규칙

### 필수 Annotation

* `[HttpGet]`, `[HttpPost]`, `[HttpPut]`, `[HttpDelete]`
* `[ProducesResponseType]`
* `[SwaggerOperation]`

### Error Response 정의

```csharp
[ProducesErrorCodes(
    StatusCodes.Status400BadRequest,
    StatusCodes.Status401Unauthorized,
    StatusCodes.Status403Forbidden,
    StatusCodes.Status409Conflict
)]
```

Controller는 **Swagger 문서의 최종 책임자**다.

---

## 5. 권한 처리 규칙 (AuthorizePermission)

```csharp
[AuthorizePermission(
    roles: new[] { EnumRole.Admin },
    permissions: new[] { EnumPermission.Dashboard_READ }
)]
```

### 권한 평가 규칙

* **Role 조건: OR**

  * 여러 Role 중 하나라도 만족하면 통과
* **Permission 조건: AND**

  * 명시된 Permission을 모두 보유해야 통과

권한 판단 로직은 모두 Middleware/Handler에서 처리한다.

Controller에서는 결과만 소비한다.

---

## 6. 인증된 사용자 정보 접근 (UserContext)

인증이 완료된 요청에는 `HttpContext.Items`에 사용자 정보가 주입된다.

```csharp
var userContext = HttpContext.Items["UserContext"] as UserContext;
```

### 사용 가능 정보

* `userContext.Uuid`  → 인증된 사용자 UUID
* `userContext.Role`  → 사용자 Role

해당 정보는 **Controller 또는 Service에서 사용 가능**하다.

---

## 7. 요청 추적 (Correlation ID)

모든 요청에는 Correlation ID가 할당된다.

```csharp
var correlationId = HttpContext.Items["X-Correlation-Id"]?.ToString();
```

### 목적

* 동일 요청의 **Request → Service → DB → Response** 전 과정 추적
* 로그 상관관계 확보
* 장애 분석 및 감사 로그 기반 제공

Correlation ID는 Middleware에서 생성·관리된다.

---

## 8. Controller 구현 예시

```csharp
[HttpGet("all")]
[AuthorizePermission(
    roles: new[] { EnumRole.Admin },
    permissions: new[] { EnumPermission.Dashboard_READ }
)]
[ProducesResponseType(typeof(BasePaginationSTO<GetAllUserSTO>), StatusCodes.Status200OK)]
[ProducesErrorCodes(StatusCodes.Status401Unauthorized, StatusCodes.Status403Forbidden)]
[SwaggerOperation(
    Summary = "Get all users list without deleted users",
    Description = "Only who has permission of super-admin can access this API"
)]
public async Task<IActionResult> GetAllUsers(GetAllUsersQTO qto)
{
    var userContext = HttpContext.Items["UserContext"] as UserContext;
    _logger.LogInformation(
        "user UUID : {UUID}, Role : {Role}",
        userContext?.Uuid,
        userContext?.Role
    );

    var solvedResults = await _authService.GetAllUser(qto);
    return Ok(solvedResults);
}
```

---

## 9. 금지 사항 ❌

* try-catch 직접 처리
* 비즈니스 로직 작성
* Entity 반환
* 권한 판별 로직 직접 구현
* HttpContext 직접 조작

---

## 10. 목적

* Controller 단순화
* Security / Authorization 일관성 유지
* Swagger 문서 품질 보장
* Request 전 구간 추적 가능
* 장기 운영 기준 확립
