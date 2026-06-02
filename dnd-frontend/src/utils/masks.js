export const maskCPF = (v) =>
  v.replace(/\D/g, '')
    .replace(/(\d{3})(\d)/, '$1.$2')
    .replace(/(\d{3})(\d)/, '$1.$2')
    .replace(/(\d{3})(\d{1,2})/, '$1-$2')
    .slice(0, 14)

export const maskCNPJ = (v) =>
  v.replace(/\D/g, '')
    .replace(/(\d{2})(\d)/, '$1.$2')
    .replace(/(\d{3})(\d)/, '$1.$2')
    .replace(/(\d{3})(\d)/, '$1/$2')
    .replace(/(\d{4})(\d{1,2})/, '$1-$2')
    .slice(0, 18)

export const maskPhone = (v) =>
  v.replace(/\D/g, '')
    .replace(/(\d{2})(\d)/, '($1) $2')
    .replace(/(\d{5})(\d)/, '$1-$2')
    .slice(0, 15)

export const maskCard = (v) =>
  v.replace(/\D/g, '')
    .replace(/(\d{4})(\d)/, '$1 $2')
    .replace(/(\d{4})(\d)/, '$1 $2')
    .replace(/(\d{4})(\d)/, '$1 $2')
    .slice(0, 19)

export const maskExpiry = (v) =>
  v.replace(/\D/g, '')
    .replace(/(\d{2})(\d)/, '$1/$2')
    .slice(0, 5)

export const stripMask = (v) => v.replace(/\D/g, '')

// Máscara automática: CPF (até 11 dígitos) ou CNPJ (acima de 11)
export const maskDocument = (v) => {
  const digits = v.replace(/\D/g, '').slice(0, 14)
  if (digits.length <= 11) return maskCPF(digits)
  return maskCNPJ(digits)
}

export const maskCurrency = (v) =>
  `R$ ${Number(v).toLocaleString('pt-BR', { minimumFractionDigits: 2 })}`
