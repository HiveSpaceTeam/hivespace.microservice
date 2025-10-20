namespace HiveSpace.UserService.Domain.Models;

public class EmailVerificationModel
{
    public string UserName { get; set; } = string.Empty;
    public string VerificationLink { get; set; } = string.Empty;
    public string AppName { get; set; } = "HiveSpace";
    public DateTime ExpiresAt { get; set; }
}

public static class EmailTemplates
{
    public const string VerificationEmailTemplate = @"
@model HiveSpace.UserService.Domain.Models.EmailVerificationModel
<!DOCTYPE html>
<html>
<head>
    <title>Account Activation - @Model.AppName</title>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
</head>
<body style='font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 20px; margin: 0;'>
    <div style='max-width: 600px; margin: auto; background: white; padding: 30px; border-radius: 8px; box-shadow: 0 4px 6px rgba(0,0,0,0.1);'>
        <div style='text-align: center; margin-bottom: 30px;'>
            <h1 style='color: #004d99; margin: 0; font-size: 28px;'>@Model.AppName</h1>
        </div>
        
        <h2 style='color: #004d99; margin-bottom: 20px;'>Hello @Model.UserName,</h2>
        
        <p style='font-size: 16px; line-height: 1.5; color: #333; margin-bottom: 20px;'>
            Thank you for choosing @Model.AppName! Please confirm your email address by clicking the link below. We'll communicate important updates with you from time to time via email, so it's essential that we have an up-to-date email address on file.
        </p>

        <div style='margin: 30px 0; text-align: center;'>
            <a href='@Model.VerificationLink' 
               style='background-color: #007bff; 
                      color: white; 
                      padding: 12px 25px; 
                      text-decoration: none; 
                      border-radius: 6px; 
                      font-size: 17px; 
                      font-weight: bold;
                      display: inline-block;
                      transition: background-color 0.3s;
                      border: none;
                      cursor: pointer;'>
                ✉️ Activate My Account
            </a>
        </div>

        <p style='font-size: 14px; color: #666; margin-bottom: 15px;'>
            If the button above doesn't work, copy and paste the following link into your browser:
        </p>
        <div style='background-color: #f8f9fa; padding: 15px; border-radius: 4px; margin-bottom: 20px;'>
            <p style='font-size: 12px; color: #555; word-break: break-all; margin: 0; font-family: monospace;'>
                @Model.VerificationLink
            </p>
        </div>
        
        <div style='background-color: #fff3cd; border: 1px solid #ffeaa7; border-radius: 4px; padding: 15px; margin: 20px 0;'>
            <p style='margin: 0; font-size: 14px; color: #856404;'>
                ⏰ <strong>Important:</strong> This verification link will expire on @Model.ExpiresAt.ToString(""MMM dd, yyyy 'at' HH:mm"") UTC. Please verify your email before then.
            </p>
        </div>
        
        <hr style='border: 0; border-top: 1px solid #eee; margin: 30px 0;'/>
        
        <div style='text-align: center;'>
            <p style='font-size: 12px; color: #777; margin: 0;'>
                This email was sent by @Model.AppName. If you didn't request this verification, you can safely ignore this email.
            </p>
            <p style='font-size: 12px; color: #777; margin: 5px 0 0 0;'>
                &copy; @Model.ExpiresAt.Year @Model.AppName. All rights reserved.
            </p>
        </div>
    </div>
</body>
</html>";

    public const string VerificationSuccessTemplate = @"
@model HiveSpace.UserService.Domain.Models.EmailVerificationModel
<!DOCTYPE html>
<html>
<head>
    <title>Email Verified Successfully - @Model.AppName</title>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
</head>
<body style='font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 20px; margin: 0;'>
    <div style='max-width: 600px; margin: auto; background: white; padding: 30px; border-radius: 8px; box-shadow: 0 4px 6px rgba(0,0,0,0.1);'>
        <div style='text-align: center; margin-bottom: 30px;'>
            <h1 style='color: #28a745; margin: 0; font-size: 28px;'>✅ Email Verified!</h1>
        </div>
        
        <h2 style='color: #004d99; margin-bottom: 20px;'>Welcome to @Model.AppName, @Model.UserName!</h2>
        
        <p style='font-size: 16px; line-height: 1.5; color: #333; margin-bottom: 20px;'>
            Congratulations! Your email address has been successfully verified. Your account is now fully activated.
        </p>

        <div style='background-color: #d4edda; border: 1px solid #c3e6cb; border-radius: 4px; padding: 15px; margin: 20px 0;'>
            <p style='margin: 0; font-size: 14px; color: #155724;'>
                🎉 <strong>What's next?</strong> You can now access all features of @Model.AppName with your verified account.
            </p>
        </div>
        
        <hr style='border: 0; border-top: 1px solid #eee; margin: 30px 0;'/>
        
        <div style='text-align: center;'>
            <p style='font-size: 12px; color: #777; margin: 5px 0 0 0;'>
                &mdash; The @Model.AppName Team
            </p>
        </div>
    </div>
</body>
</html>";
}