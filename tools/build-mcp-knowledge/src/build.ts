import * as fs from "fs";
import * as path from "path";
import { fileURLToPath } from "url";

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const REPO_ROOT = path.resolve(__dirname, "../../..");
const SRC_DIR = path.join(REPO_ROOT, "src");
const AI_AGENTS_FILE = path.join(REPO_ROOT, "ai", "AGENTS.md");
const OUTPUT_FILE = path.join(REPO_ROOT, "tools", "mcp-server", "knowledge-base.json");

// File name suffixes → content type keys
const SUFFIX_MAP: Record<string, string> = {
  ".instructions": "instructions",
  ".examples": "examples",
  ".setup": "setup",
  ".signatures": "signatures",
  ".namespaces": "namespaces",
  ".advanced.example": "advanced_examples",
};

interface PackageEntry {
  id: string;
  version: string;
  description: string;
  tags: string[];
  files: Record<string, string>;
}

function readCsprojMeta(csprojPath: string) {
  const content = fs.readFileSync(csprojPath, "utf8");
  const get = (tag: string) => content.match(new RegExp(`<${tag}>([^<]*)<\/${tag}>`))?.[1]?.trim() ?? "";
  return {
    id: get("PackageId") || path.basename(csprojPath, ".csproj"),
    version: get("Version") || "0.0.0",
    description: get("Description"),
    tags: get("PackageTags").split(";").map((t) => t.trim()).filter(Boolean),
  };
}

function classifyFile(name: string): string | null {
  // Skip subagent files — those are for .claude/agents/, not consumer guidance
  if (name.endsWith("-agent.md")) return null;
  const base = name.replace(/\.md$/, "");
  for (const [suffix, key] of Object.entries(SUFFIX_MAP)) {
    if (base.endsWith(suffix)) return key;
  }
  // Unknown suffix: use the base name as the key (catches sub-domain files like office.barcodes.instructions)
  return base;
}

function collectPackages(): PackageEntry[] {
  const entries: PackageEntry[] = [];

  for (const dirent of fs.readdirSync(SRC_DIR, { withFileTypes: true })) {
    if (!dirent.isDirectory()) continue;
    const dir = path.join(SRC_DIR, dirent.name);
    const csprojFiles = fs.readdirSync(dir).filter((f) => f.endsWith(".csproj"));
    if (csprojFiles.length === 0) continue;

    const meta = readCsprojMeta(path.join(dir, csprojFiles[0]));
    const files: Record<string, string> = {};

    const aiDir = path.join(dir, "ai");
    if (fs.existsSync(aiDir)) {
      for (const file of fs.readdirSync(aiDir)) {
        if (!file.endsWith(".md")) continue;
        const key = classifyFile(file);
        if (!key) continue;
        files[key] = fs.readFileSync(path.join(aiDir, file), "utf8");
      }
    }

    entries.push({ ...meta, files });
  }

  return entries.sort((a, b) => a.id.localeCompare(b.id));
}

const packages = collectPackages();
const bootstrap = fs.readFileSync(AI_AGENTS_FILE, "utf8");
const withDocs = packages.filter((p) => Object.keys(p.files).length > 0).length;

const knowledgeBase = {
  generated: new Date().toISOString(),
  packageCount: packages.length,
  packagesWithDocs: withDocs,
  packages,
  bootstrap,
};

fs.mkdirSync(path.dirname(OUTPUT_FILE), { recursive: true });
fs.writeFileSync(OUTPUT_FILE, JSON.stringify(knowledgeBase, null, 2));
console.log(
  `Generated knowledge-base.json: ${packages.length} packages (${withDocs} with AI docs)`
);
