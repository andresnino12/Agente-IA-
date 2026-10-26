import fs from "node:fs/promises";
import { FileBlob, SpreadsheetFile } from "@oai/artifact-tool";

const inputPath = "C:/Users/andre/AppData/Local/Packages/5319275A.WhatsAppDesktop_cv1g1gvanyjgm/LocalState/sessions/05205DEA17F589057AC44D9292774F41665E8FAE/transfers/2026-43/7 - CLASE 7 - MATERIAL DE APOYO MODELO GUIA PLAN SEMANAL DE ALIMENTACION.xlsx";
const outputDir = "C:/Users/andre/OneDrive/Documentos/trabajos sena/trimestre 5/agentes/Agentes_La_LLeva/outputs/plan_semanal_alimentacion";
const mode = process.argv[2] ?? "inspect";
const completedPath = `${outputDir}/plan_semanal_alimentacion_completado.xlsx`;
const sourcePath = mode === "verify" ? completedPath : inputPath;

const input = await FileBlob.load(sourcePath);
const workbook = await SpreadsheetFile.importXlsx(input);
const sheets = await workbook.inspect({ kind: "workbook,sheet,table", maxChars: 8000, tableMaxRows: 50, tableMaxCols: 12, tableMaxCellChars: 80 });
console.log(sheets.ndjson);

for (const name of ["Hoja1", "Sheet1"]) {
  try {
    const preview = await workbook.render({ sheetName: name, autoCrop: "all", scale: 1.5, format: "png" });
    await fs.mkdir(outputDir, { recursive: true });
    await fs.writeFile(`${outputDir}/${mode === "verify" ? "after" : "before"}.png`, new Uint8Array(await preview.arrayBuffer()));
    console.log(`Rendered ${name}`);
    break;
  } catch {}
}

if (mode === "verify") {
  const values = await workbook.inspect({
    kind: "table",
    range: "Hoja1!A1:F43",
    include: "values,formulas",
    tableMaxRows: 43,
    tableMaxCols: 6,
    maxChars: 12000,
  });
  console.log(values.ndjson);
  const errors = await workbook.inspect({
    kind: "match",
    searchTerm: "#REF!|#DIV/0!|#VALUE!|#NAME\\?|#N/A|#NUM!|#NULL!|#SPILL!|#CALC!",
    options: { useRegex: true, maxResults: 100 },
    summary: "final formula error scan",
  });
  console.log(errors.ndjson);
  process.exit(0);
}

if (mode !== "edit") process.exit(0);

const sheet = workbook.worksheets.getItem("Hoja1");
const colors = {
  C: "#00B050", // Carbohidratos
  V: "#7030A0", // Vitaminas
  M: "#00B0F0", // Minerales
  P: "#FF0000", // Proteínas
  L: "#FFC000", // Lípidos
};

// Cada fila corresponde a un alimento de la comida indicada en la columna.
// C: carbohidratos, V: vitaminas, M: minerales, P: proteínas, L: lípidos.
const weeks = [
  [
    [["Arepa", "C"], ["Manzana", "V"], ["Arroz", "C"], ["Banano", "V"], ["Sopa verdura", "V"]],
    [["Huevo", "P"], ["Yogur", "M"], ["Pollo", "P"], ["Yogur", "M"], ["Pan", "C"]],
    [["Leche", "M"], ["Avena", "C"], ["Frijoles", "M"], ["Galletas", "C"], ["Queso", "P"]],
    [["Papaya", "V"], ["Maní", "L"], ["Ensalada", "V"], ["Maní", "L"], ["Leche", "M"]],
    [["Aguacate", "L"], ["Queso", "P"], ["Aguacate", "L"], ["Huevo", "P"], ["Aceite oliva", "L"]],
  ],
  [
    [["Pan integral", "C"], ["Pera", "V"], ["Lentejas", "P"], ["Naranja", "V"], ["Arepa", "C"]],
    [["Queso", "P"], ["Yogur", "M"], ["Arroz", "C"], ["Yogur", "M"], ["Huevo", "P"]],
    [["Leche", "M"], ["Granola", "C"], ["Ensalada", "V"], ["Galletas", "C"], ["Tomate", "V"]],
    [["Banano", "V"], ["Nueces", "L"], ["Aguacate", "L"], ["Nueces", "L"], ["Queso", "P"]],
    [["Aguacate", "L"], ["Queso", "P"], ["Agua", "M"], ["Manzana", "V"], ["Aceite oliva", "L"]],
  ],
  [
    [["Avena", "C"], ["Mandarina", "V"], ["Papa", "C"], ["Yogur", "M"], ["Sándwich", "C"]],
    [["Leche", "M"], ["Yogur", "M"], ["Carne", "P"], ["Manzana", "V"], ["Pollo", "P"]],
    [["Huevo", "P"], ["Galletas", "C"], ["Ensalada", "V"], ["Galletas", "C"], ["Tomate", "V"]],
    [["Manzana", "V"], ["Maní", "L"], ["Frijoles", "M"], ["Maní", "L"], ["Queso", "P"]],
    [["Maní", "L"], ["Queso", "P"], ["Aguacate", "L"], ["Banano", "V"], ["Aguacate", "L"]],
  ],
  [
    [["Arepa", "C"], ["Papaya", "V"], ["Arroz", "C"], ["Pera", "V"], ["Ensalada", "V"]],
    [["Huevo", "P"], ["Yogur", "M"], ["Pescado", "P"], ["Yogur", "M"], ["Atún", "P"]],
    [["Leche", "M"], ["Pan integral", "C"], ["Verduras", "V"], ["Galletas", "C"], ["Galletas", "C"]],
    [["Fresa", "V"], ["Nueces", "L"], ["Lentejas", "M"], ["Nueces", "L"], ["Tomate", "V"]],
    [["Aguacate", "L"], ["Queso", "P"], ["Aguacate", "L"], ["Queso", "P"], ["Aceite oliva", "L"]],
  ],
  [
    [["Cereal", "C"], ["Banano", "V"], ["Pasta", "C"], ["Yogur", "M"], ["Sopa", "V"]],
    [["Leche", "M"], ["Yogur", "M"], ["Pollo", "P"], ["Galletas", "C"], ["Pollo", "P"]],
    [["Huevo", "P"], ["Avena", "C"], ["Ensalada", "V"], ["Manzana", "V"], ["Pan", "C"]],
    [["Manzana", "V"], ["Maní", "L"], ["Frijoles", "M"], ["Pastel", "C"], ["Queso", "P"]],
    [["Maní", "L"], ["Queso", "P"], ["Aguacate", "L"], ["Maní", "L"], ["Aguacate", "L"]],
  ],
  [
    [["Arroz", "C"], ["Mandarina", "V"], ["Yuca", "C"], ["Yogur", "M"], ["Pizza", "C"]],
    [["Frijoles", "M"], ["Yogur", "M"], ["Carne", "P"], ["Banano", "V"], ["Queso", "P"]],
    [["Huevo", "P"], ["Galletas", "C"], ["Ensalada", "V"], ["Galletas", "C"], ["Tomate", "V"]],
    [["Papaya", "V"], ["Maní", "L"], ["Aguacate", "L"], ["Nueces", "L"], ["Aceite oliva", "L"]],
    [["Aguacate", "L"], ["Queso", "P"], ["Agua", "M"], ["Queso", "P"], ["Ensalada", "V"]],
  ],
  [
    [["Arepa", "C"], ["Fruta", "V"], ["Arroz", "C"], ["Leche", "M"], ["Sándwich", "C"]],
    [["Queso", "P"], ["Yogur", "M"], ["Pollo", "P"], ["Galletas", "C"], ["Queso", "P"]],
    [["Leche", "M"], ["Granola", "C"], ["Ensalada", "V"], ["Manzana", "V"], ["Tomate", "V"]],
    [["Mango", "V"], ["Nueces", "L"], ["Frijoles", "M"], ["Maní", "L"], ["Aguacate", "L"]],
    [["Aguacate", "L"], ["Queso", "P"], ["Aguacate", "L"], ["Postre", "C"], ["Aceite oliva", "L"]],
  ],
];

weeks.forEach((day, dayIndex) => {
  const startRow = 3 + (dayIndex * 5);
  const values = Array.from({ length: 5 }, (_, row) =>
    Array.from({ length: 5 }, (_, col) => day[row][col][0])
  );
  sheet.getRange(`B${startRow}:F${startRow + 4}`).values = values;
  day.forEach((row, rowIndex) => row.forEach(([_, category], colIndex) => {
    sheet.getCell(startRow - 1 + rowIndex, 1 + colIndex).format.fill = colors[category];
  }));
});

sheet.getRange("B3:F37").format.font = { name: "Arial", size: 9, color: "#000000" };
sheet.getRange("B3:F37").format.verticalAlignment = "center";
workbook.recalculate();
await fs.mkdir(outputDir, { recursive: true });
const output = await SpreadsheetFile.exportXlsx(workbook);
await output.save(completedPath);
