window.inspector = {
    selection: [],
    setSelection(names) {
        this.selection = Array.isArray(names) ? names : [];
    },
    downloadText: (filename, content, type) => {
        const blob = new Blob([content], { type: type || "text/plain" });
        const url = URL.createObjectURL(blob);
        const link = document.createElement("a");
        link.href = url;
        link.download = filename;
        document.body.appendChild(link);
        link.click();
        link.remove();
        URL.revokeObjectURL(url);
    }
};

(() => {
    const context = document.modelContext;
    if (!context?.registerTool) return;

    try {
        Promise.resolve(context.registerTool({
            name: "export_selected_handoff",
            title: "Export selected cleaner entries",
            description: "Download the cleaner entries currently selected in FluentCleaner Inspector as a compatible handoff JSON file.",
            inputSchema: { type: "object", properties: {}, additionalProperties: false },
            annotations: { readOnlyHint: false, untrustedContentHint: true },
            execute() {
                const names = window.inspector.selection;
                if (!names.length) throw new Error("No cleaner entries are selected.");
                window.inspector.downloadText(
                    "fluentcleaner-selection.json",
                    JSON.stringify([...names].sort((a, b) => a.localeCompare(b)), null, 2),
                    "application/json"
                );
                return { exportedEntries: names.length };
            }
        })).catch(() => {});
    } catch { }
})();
