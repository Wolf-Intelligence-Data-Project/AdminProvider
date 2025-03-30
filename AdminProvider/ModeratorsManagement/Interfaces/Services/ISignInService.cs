using AdminProvider.ModeratorsManagement.Models.Responses;
using AuthenticationProvider.Models.Requests;

namespace AdminProvider.ModeratorsManagement.Interfaces.Services;

public interface ISignInService
{

    Task<SignInResponse> SignInAsync(SignInRequest signInRequest);


}
