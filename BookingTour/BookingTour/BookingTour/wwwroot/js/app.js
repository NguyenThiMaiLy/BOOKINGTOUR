//const sign_in_btn = document.querySelector("#sign-in-btn");
//const sign_up_btn = document.querySelector("#sign-up-btn");
//const container = document.querySelector(".container");

//sign_up_btn.addEventListener("click", () => {
 // container.classList.add("sign-up-mode");
//});

//sign_in_btn.addEventListener("click", () => {
 // container.classList.remove("sign-up-mode");
//});
// Form liên quan đến quên mật khẩu
const sign_in_btn = document.querySelector("#sign-in-btn");
const sign_up_btn = document.querySelector("#sign-up-btn");
const container = document.querySelector(".container");

// Form liên quan đến quên mật khẩu
const forgot_password_link = document.querySelector(".forgot-password");
const back_to_signin_link = document.querySelector(".back-to-signin");
const sign_in_form = document.querySelector(".sign-in-form");
const sign_up_form = document.querySelector(".sign-up-form");
const forgot_password_form = document.querySelector(".forgot-password-form");

// Chuyển sang chế độ đăng ký
sign_up_btn.addEventListener("click", () => {
  container.classList.add("sign-up-mode");
  // Ẩn form quên mật khẩu nếu đang hiển thị
  forgot_password_form.classList.remove("show");
  sign_in_form.classList.remove("hidden");
});

// Chuyển sang chế độ đăng nhập
sign_in_btn.addEventListener("click", () => {
  container.classList.remove("sign-up-mode");
  // Ẩn form quên mật khẩu nếu đang hiển thị
  forgot_password_form.classList.remove("show");
  sign_in_form.classList.remove("hidden");
});

// Khi nhấn vào "Forgot Password?"
forgot_password_link.addEventListener("click", (e) => {
  e.preventDefault();
  sign_in_form.classList.add("hidden");
  sign_up_form.classList.add("hidden"); // Ẩn form đăng ký nếu cần
  forgot_password_form.classList.add("show");
});

// Khi nhấn vào "Back to Sign In"
back_to_signin_link.addEventListener("click", (e) => {
  e.preventDefault();
  forgot_password_form.classList.remove("show");
  sign_in_form.classList.remove("hidden");
  sign_up_form.classList.remove("hidden"); // Hiển thị lại form đăng ký nếu cần
});
