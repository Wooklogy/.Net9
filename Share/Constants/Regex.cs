namespace Share.Constants;

public static class RegexConstants
{
    // 아이디: 4~20자 영문/숫자
    public const string IdentifyRegex = @"^[a-zA-Z0-9]{4,20}$";

    // 비밀번호: 8~32자 영문, 숫자, 특수문자 조합
    public const string PasswordRegex = @"^(?=.*[A-Za-z])(?=.*\d)(?=.*[!@#$%^&*()_\-+=\[\]{};:'"",.<>/?\\|~`])[A-Za-z\d!@#$%^&*()_\-+=\[\]{};:'"",.<>/?\\|~`]{8,32}$";
}