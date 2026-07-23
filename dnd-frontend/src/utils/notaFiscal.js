import { format } from 'date-fns'
import { ptBR } from 'date-fns/locale'

const fmt = (v) => `R$ ${Number(v).toLocaleString('pt-BR', { minimumFractionDigits: 2 })}`

export function gerarNotaFiscal(order, { mostrarLucro = false } = {}) {
  const dataFormatada = format(new Date(order.createdAt), "dd/MM/yyyy 'às' HH:mm", { locale: ptBR })

  const linhasItens = (order.items || []).map((item, i) => `
    <tr class="${i % 2 === 0 ? 'par' : ''}">
      <td>${item.productName || '—'}</td>
      <td class="center">${item.quantity}</td>
      <td class="right">${fmt(item.unitPrice)}</td>
      <td class="right"><strong>${fmt(item.unitPrice * item.quantity)}</strong></td>
      ${mostrarLucro ? `
        <td class="right">${fmt(item.unitCost)}</td>
        <td class="right profit">${fmt(item.profit)}</td>
      ` : ''}
    </tr>
  `).join('')

  const html = `<!DOCTYPE html>
<html lang="pt-BR">
<head>
  <meta charset="UTF-8" />
  <title>Nota Fiscal — Pedido #${order.id}</title>
  <style>
    * { margin: 0; padding: 0; box-sizing: border-box; }
    body { font-family: Arial, sans-serif; font-size: 12px; color: #1a1a2e; padding: 32px; background: #fff; }

    .header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 24px; padding-bottom: 16px; border-bottom: 2px solid #1e3a8a; }
    .logo-area h1 { font-size: 22px; font-weight: 800; color: #1e3a8a; letter-spacing: -0.5px; }
    .logo-area p { font-size: 11px; color: #64748b; margin-top: 2px; }
    .doc-info { text-align: right; }
    .doc-info .doc-title { font-size: 16px; font-weight: 700; color: #1e3a8a; text-transform: uppercase; letter-spacing: 1px; }
    .doc-info .doc-number { font-size: 20px; font-weight: 800; color: #334155; margin-top: 2px; }
    .doc-info .doc-date { font-size: 11px; color: #64748b; margin-top: 3px; }

    .status-badge { display: inline-block; padding: 3px 10px; border-radius: 20px; font-size: 10px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.5px; background: #dbeafe; color: #1d4ed8; margin-top: 4px; }

    .section { margin-bottom: 20px; }
    .section-title { font-size: 10px; font-weight: 700; color: #64748b; text-transform: uppercase; letter-spacing: 1px; margin-bottom: 8px; padding-bottom: 4px; border-bottom: 1px solid #e2e8f0; }

    .info-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 8px 24px; }
    .info-item label { font-size: 10px; color: #94a3b8; display: block; margin-bottom: 1px; }
    .info-item span { font-size: 12px; font-weight: 600; color: #334155; }

    table { width: 100%; border-collapse: collapse; margin-top: 4px; }
    thead tr { background: #1e3a8a; color: white; }
    thead th { padding: 8px 10px; text-align: left; font-size: 10px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.5px; }
    th.center, td.center { text-align: center; }
    th.right, td.right { text-align: right; }
    tbody tr { border-bottom: 1px solid #f1f5f9; }
    tbody tr.par { background: #f8fafc; }
    tbody td { padding: 8px 10px; font-size: 11px; color: #374151; }
    td.profit { color: #16a34a; font-weight: 700; }

    .totals { margin-top: 16px; display: flex; justify-content: flex-end; }
    .totals-box { min-width: 240px; }
    .totals-row { display: flex; justify-content: space-between; padding: 5px 0; font-size: 12px; border-bottom: 1px solid #f1f5f9; }
    .totals-row.total { border-top: 2px solid #1e3a8a; border-bottom: none; padding-top: 10px; margin-top: 4px; }
    .totals-row.total span:first-child { font-size: 13px; font-weight: 700; color: #1e3a8a; }
    .totals-row.total span:last-child { font-size: 16px; font-weight: 800; color: #1e3a8a; }
    .totals-row .profit-label { color: #16a34a; font-weight: 700; }
    .totals-row .profit-value { color: #16a34a; font-weight: 700; }

    .footer { margin-top: 32px; padding-top: 16px; border-top: 1px solid #e2e8f0; display: flex; justify-content: space-between; align-items: center; }
    .footer p { font-size: 10px; color: #94a3b8; }
    .footer .generated { font-size: 10px; color: #cbd5e1; }

    @media print {
      body { padding: 16px; }
      @page { margin: 1cm; size: A4; }
    }
  </style>
</head>
<body>
  <div class="header">
    <div class="logo-area">
      <h1>D&amp;D Distribuidora</h1>
      <p>Distribuidora Atacadista</p>
    </div>
    <div class="doc-info">
      <div class="doc-title">Comprovante de Venda</div>
      <div class="doc-number">#${String(order.id).padStart(5, '0')}</div>
      <div class="doc-date">${dataFormatada}</div>
      <div class="status-badge">${order.status || 'Pendente'}</div>
    </div>
  </div>

  <div class="section">
    <div class="section-title">Dados do Comprador</div>
    <div class="info-grid">
      <div class="info-item"><label>Nome</label><span>${order.buyerName || '—'}</span></div>
      <div class="info-item"><label>Loja / Empresa</label><span>${order.buyerStoreName || '—'}</span></div>
      <div class="info-item"><label>Telefone</label><span>${order.buyerPhone || '—'}</span></div>
      ${order.attendedBySellerName ? `<div class="info-item"><label>Atendido por</label><span>${order.attendedBySellerName}</span></div>` : ''}
    </div>
  </div>

  <div class="section">
    <div class="section-title">Itens do Pedido</div>
    <table>
      <thead>
        <tr>
          <th style="width:40%">Produto</th>
          <th class="center" style="width:10%">Qtd.</th>
          <th class="right" style="width:15%">Preço Unit.</th>
          <th class="right" style="width:15%">Subtotal</th>
          ${mostrarLucro ? '<th class="right" style="width:10%">Custo Unit.</th><th class="right" style="width:10%">Lucro</th>' : ''}
        </tr>
      </thead>
      <tbody>
        ${linhasItens}
      </tbody>
    </table>
  </div>

  <div class="totals">
    <div class="totals-box">
      <div class="totals-row">
        <span>Subtotal</span>
        <span>${fmt(order.totalAmount)}</span>
      </div>
      ${mostrarLucro ? `
      <div class="totals-row">
        <span class="profit-label">Lucro total</span>
        <span class="profit-value">${fmt(order.totalProfit)}</span>
      </div>` : ''}
      <div class="totals-row total">
        <span>TOTAL</span>
        <span>${fmt(order.totalAmount)}</span>
      </div>
    </div>
  </div>

  <div class="footer">
    <p>D&amp;D Distribuidora — Este documento é um comprovante interno de venda.</p>
    <p class="generated">Gerado em ${format(new Date(), "dd/MM/yyyy 'às' HH:mm", { locale: ptBR })}</p>
  </div>

  <script>window.onload = () => window.print()</script>
</body>
</html>`

  const janela = window.open('', '_blank', 'width=900,height=700')
  janela.document.write(html)
  janela.document.close()
}
