import { API_URL } from './config'

// Dispara uma chamada "silenciosa" ao backend assim que o site carrega, antes
// de qualquer ação do usuário. Se o servidor estiver hibernando (plano free),
// isso já inicia o "acordar" em paralelo com o usuário lendo a tela/digitando
// login — quando ele de fato submeter uma ação, o backend tem mais chance de
// já estar de pé. Não bloqueia nada e ignora qualquer erro.
export function wakeBackend() {
  if (!API_URL) return
  fetch(`${API_URL}/health`).catch(() => {})
}
