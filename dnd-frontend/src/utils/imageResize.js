// Redimensiona/comprimir uma imagem no navegador antes do upload, via Canvas.
// Evita subir fotos de celular de vários MB direto pro storage — o comprador
// que abre a loja depois é quem paga esse custo em tempo de carregamento.
export function resizeImage(file, { maxDimension = 1600, quality = 0.82 } = {}) {
  return new Promise((resolve, reject) => {
    if (!file.type.startsWith('image/')) { resolve(file); return }

    const img = new Image()
    const url = URL.createObjectURL(file)

    img.onload = () => {
      URL.revokeObjectURL(url)

      let { width, height } = img
      if (width <= maxDimension && height <= maxDimension) {
        resolve(file) // já é pequena o suficiente, não precisa reprocessar
        return
      }

      const scale = maxDimension / Math.max(width, height)
      width = Math.round(width * scale)
      height = Math.round(height * scale)

      const canvas = document.createElement('canvas')
      canvas.width = width
      canvas.height = height
      canvas.getContext('2d').drawImage(img, 0, 0, width, height)

      canvas.toBlob(blob => {
        if (!blob) { resolve(file); return }
        const nomeComprimido = file.name.replace(/\.\w+$/, '') + '.jpg'
        resolve(new File([blob], nomeComprimido, { type: 'image/jpeg' }))
      }, 'image/jpeg', quality)
    }

    img.onerror = () => { URL.revokeObjectURL(url); resolve(file) } // se falhar, sobe o original
    img.src = url
  })
}
