export class TransactionForm {
    public static onLoad(executionContext: Xrm.Events.EventContext): void {
        const formContext = executionContext.getFormContext();
        const dateAttribute = formContext.getAttribute('lab_transactiondate');
        if (formContext.ui.getFormType() === 1 && dateAttribute && !dateAttribute.getValue()) {
            dateAttribute.setValue(new Date());
        }
    }

    public static async onQuantityChange(executionContext: Xrm.Events.EventContext): Promise<void> {
        const formContext = executionContext.getFormContext();
        const quantity = (formContext.getAttribute('lab_quantity') as Xrm.Attributes.NumberAttribute)?.getValue();
        const itemValue = (formContext.getAttribute('lab_itemid') as Xrm.Attributes.LookupAttribute)?.getValue();
        if (quantity == null || !itemValue?.length) return;

        try {
            const item = await Xrm.WebApi.retrieveRecord(
                'lab_warehouseitem',
                itemValue[0].id.replace(/[{}]/g, ''),
                '?$select=lab_unitprice'
            );
            const unitPrice = item['lab_unitprice'] as number | undefined;
            if (typeof unitPrice === 'number') {
                (formContext.getAttribute('lab_totalvalue') as Xrm.Attributes.NumberAttribute)?.setValue(quantity * unitPrice);
            }
        } catch {
        }
    }
}

export class RibbonActions {
    public static async checkStockLevels(formContext: Xrm.FormContext): Promise<void> {
        const name = (formContext.getAttribute('lab_name') as Xrm.Attributes.StringAttribute)?.getValue() || 'Unknown';
        const quantity = (formContext.getAttribute('lab_availablequantity') as Xrm.Attributes.NumberAttribute)?.getValue() ?? 0;
        const reorderPoint = (formContext.getAttribute('lab_reorderpoint') as Xrm.Attributes.NumberAttribute)?.getValue() ?? 0;

        const message = quantity <= reorderPoint
            ? `Stock level for ${name}: ${quantity} units.\n⚠ Below reorder point (${reorderPoint}). Consider restocking.`
            : `Stock level for ${name}: ${quantity} units.\n✓ Stock is above reorder point (${reorderPoint}).`;

        await Xrm.Navigation.openAlertDialog({ text: message, title: 'Stock Check' });
    }
}

export { GridApi, LocationForm } from './GridCustomizer';
