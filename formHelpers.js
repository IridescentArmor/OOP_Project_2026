export const phonePattern = /^\+?[0-9()\-\s]{10,32}$/;
export const personNamePattern = /^[A-Za-zА-Яа-яІіЇїЄєҐґ'’`\-\s]{2,100}$/;

export function validatePhoneNumber(value) {
  const trimmed = value.trim();
  if (!phonePattern.test(trimmed)) {
    return "Номер телефону може містити лише цифри, пробіли, +, дужки та дефіс.";
  }

  const digitsCount = trimmed.replace(/\D/g, "").length;
  if (digitsCount < 10 || digitsCount > 15) {
    return "Номер телефону має містити від 10 до 15 цифр.";
  }

  return "";
}

export function validatePersonName(value) {
  if (!personNamePattern.test(value.trim())) {
    return "Ім'я може містити лише літери, пробіли, апостроф і дефіс.";
  }

  return "";
}
