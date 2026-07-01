/* ──────────────────────────────────────────────────────────────
   BulkingPro — Modal de confirmação / alerta padronizado
   Substitui os window.confirm() / window.alert() nativos do navegador
   por um modal estilizado de acordo com o tema do site.

   Uso:
     const ok = await siteConfirm('Tem certeza que deseja excluir?');
     await siteAlert('Operação concluída com sucesso!');

   Também intercepta automaticamente qualquer <form data-confirm="mensagem">
   exibindo o modal antes do envio (substitui onsubmit="return confirm(...)").
   ─────────────────────────────────────────────────────────────── */
(function () {
    if (window.siteConfirm) return; // evita injeção duplicada

    const STYLE_ID = 'site-dialog-styles';

    function ensureStyles() {
        if (document.getElementById(STYLE_ID)) return;
        const style = document.createElement('style');
        style.id = STYLE_ID;
        style.textContent = `
            .sd-overlay {
                position: fixed; inset: 0; width: 100%; height: 100%;
                background: rgba(0,0,0,0.8);
                backdrop-filter: blur(8px);
                z-index: 5000;
                display: flex; align-items: center; justify-content: center;
                opacity: 0; transition: opacity .2s ease;
                padding: 20px;
            }
            .sd-overlay.sd-open { opacity: 1; }
            .sd-modal {
                width: 100%; max-width: 380px;
                background: var(--bg2, #0e0e10);
                border: 1px solid rgba(255,255,255,0.08);
                border-radius: 20px;
                overflow: hidden;
                box-shadow: 0 30px 60px rgba(0,0,0,0.6);
                transform: translateY(16px) scale(.97);
                transition: transform .25s cubic-bezier(.2,.9,.25,1);
                font-family: system-ui, -apple-system, 'Segoe UI', sans-serif;
            }
            .sd-overlay.sd-open .sd-modal { transform: translateY(0) scale(1); }
            .sd-icon-wrap {
                margin: 28px auto 4px; width: 52px; height: 52px;
                border-radius: 50%;
                display: flex; align-items: center; justify-content: center;
                font-size: 24px;
                background: rgba(196,168,122,0.12); color: var(--gold, #c4a87a);
            }
            .sd-modal.sd-danger .sd-icon-wrap {
                background: rgba(238,25,32,0.12); color: var(--red, #ee1920);
            }
            .sd-modal.sd-success .sd-icon-wrap {
                background: rgba(46,160,67,0.12); color: #2ea043;
            }
            .sd-body { padding: 4px 24px 24px; text-align: center; }
            .sd-title {
                font-size: 16px; font-weight: 600; color: #fff;
                margin-bottom: 8px;
            }
            .sd-message {
                font-size: 13px; line-height: 1.6;
                color: var(--muted, rgba(255,255,255,0.6));
            }
            .sd-actions {
                display: flex; gap: 10px;
                padding: 16px 24px 24px;
            }
            .sd-btn {
                flex: 1;
                display: inline-flex; align-items: center; justify-content: center;
                gap: 6px;
                border-radius: 20px;
                padding: 10px 18px;
                font-size: 13px; font-weight: 500;
                cursor: pointer; border: none;
                transition: all .2s;
                font-family: inherit;
            }
            .sd-btn:hover { opacity: .85; transform: translateY(-1px); }
            .sd-btn-ghost {
                background: rgba(255,255,255,.05);
                color: var(--muted, rgba(255,255,255,0.6));
                border: 1px solid var(--border, rgba(255,255,255,0.07));
            }
            .sd-btn-primary {
                background: linear-gradient(135deg, var(--red, #ee1920), #c41218);
                color: #fff;
            }
            @keyframes sdFadeIn { from { opacity: 0; } to { opacity: 1; } }
        `;
        document.head.appendChild(style);
    }

    function buildModal({ title, message, danger, iconClass, confirmText, cancelText, alertOnly }) {
        ensureStyles();

        const overlay = document.createElement('div');
        overlay.className = 'sd-overlay';

        const modal = document.createElement('div');
        modal.className = 'sd-modal' + (danger ? ' sd-danger' : (alertOnly ? ' sd-success' : ''));

        modal.innerHTML = `
            <div class="sd-icon-wrap"><i class="${iconClass}"></i></div>
            <div class="sd-body">
                ${title ? `<div class="sd-title">${title}</div>` : ''}
                <div class="sd-message"></div>
            </div>
            <div class="sd-actions"></div>
        `;
        modal.querySelector('.sd-message').textContent = message;

        const actions = modal.querySelector('.sd-actions');

        if (alertOnly) {
            const okBtn = document.createElement('button');
            okBtn.type = 'button';
            okBtn.className = 'sd-btn sd-btn-primary';
            okBtn.textContent = confirmText || 'OK';
            actions.appendChild(okBtn);
            overlay.appendChild(modal);
            return { overlay, modal, okBtn };
        } else {
            const cancelBtn = document.createElement('button');
            cancelBtn.type = 'button';
            cancelBtn.className = 'sd-btn sd-btn-ghost';
            cancelBtn.textContent = cancelText || 'Cancelar';

            const confirmBtn = document.createElement('button');
            confirmBtn.type = 'button';
            confirmBtn.className = 'sd-btn sd-btn-primary';
            confirmBtn.textContent = confirmText || 'Confirmar';

            actions.appendChild(cancelBtn);
            actions.appendChild(confirmBtn);
            overlay.appendChild(modal);
            return { overlay, modal, cancelBtn, confirmBtn };
        }
    }

    function openOverlay(overlay) {
        document.body.appendChild(overlay);
        document.body.style.overflow = 'hidden';
        requestAnimationFrame(() => overlay.classList.add('sd-open'));
    }

    function closeOverlay(overlay) {
        overlay.classList.remove('sd-open');
        document.body.style.overflow = '';
        setTimeout(() => overlay.remove(), 200);
    }

    /**
     * siteConfirm(message, options) -> Promise<boolean>
     * options: { title, confirmText, cancelText, danger }
     */
    window.siteConfirm = function (message, options) {
        options = options || {};
        return new Promise((resolve) => {
            const { overlay, modal, cancelBtn, confirmBtn } = buildModal({
                title: options.title || 'Confirmar ação',
                message: message,
                danger: !!options.danger,
                iconClass: options.iconClass || (options.danger ? 'ti ti-alert-triangle' : 'ti ti-help-circle'),
                confirmText: options.confirmText,
                cancelText: options.cancelText
            });

            function finish(result) {
                closeOverlay(overlay);
                document.removeEventListener('keydown', onKeyDown);
                resolve(result);
            }

            function onKeyDown(e) {
                if (e.key === 'Escape') finish(false);
            }

            cancelBtn.addEventListener('click', () => finish(false));
            confirmBtn.addEventListener('click', () => finish(true));
            overlay.addEventListener('click', (e) => { if (e.target === overlay) finish(false); });
            document.addEventListener('keydown', onKeyDown);

            openOverlay(overlay);
            confirmBtn.focus();
        });
    };

    /**
     * siteAlert(message, options) -> Promise<void>
     * options: { title, okText, danger }
     */
    window.siteAlert = function (message, options) {
        options = options || {};
        return new Promise((resolve) => {
            const { overlay, okBtn } = buildModal({
                title: options.title || (options.danger ? 'Ops, algo deu errado' : 'Aviso'),
                message: message,
                danger: !!options.danger,
                iconClass: options.iconClass || (options.danger ? 'ti ti-alert-circle' : 'ti ti-circle-check'),
                confirmText: options.okText || 'OK',
                alertOnly: true
            });

            function finish() {
                closeOverlay(overlay);
                document.removeEventListener('keydown', onKeyDown);
                resolve();
            }

            function onKeyDown(e) {
                if (e.key === 'Escape' || e.key === 'Enter') finish();
            }

            okBtn.addEventListener('click', finish);
            overlay.addEventListener('click', (e) => { if (e.target === overlay) finish(); });
            document.addEventListener('keydown', onKeyDown);

            openOverlay(overlay);
            okBtn.focus();
        });
    };

    /* ── Intercepta forms com data-confirm="mensagem" ──
       Substitui o padrão onsubmit="return confirm('...')" */
    document.addEventListener('submit', async function (e) {
        const form = e.target;
        if (!(form instanceof HTMLFormElement)) return;
        if (!form.hasAttribute('data-confirm') || form.dataset.sdConfirmed === 'true') return;

        e.preventDefault();
        const message = form.getAttribute('data-confirm');
        const danger = form.hasAttribute('data-confirm-danger');
        const ok = await siteConfirm(message, {
            danger: danger,
            title: form.getAttribute('data-confirm-title') || undefined,
            confirmText: form.getAttribute('data-confirm-text') || undefined
        });

        if (ok) {
            form.dataset.sdConfirmed = 'true';
            form.submit();
        }
    });
})();
