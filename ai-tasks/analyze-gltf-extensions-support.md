## **Task Title**  
Analyze StraightFour Entities and Map Support for OMI glTF Extensions

---

## **Purpose**  
StraightFour defines a set of entity types used across the spatial ecosystem.  
The OMI glTF Extensions repository defines a wide range of glTF extension specifications that describe additional capabilities, metadata, behaviors, and semantics.

This task aims to:

- Extract and catalog all entity types defined in the StraightFour repository  
- Clone and read (but not modify) the OMI glTF Extensions repository  
- Extract and catalog all extensions and their features  
- Map StraightFour entity types to the glTF extensions they correspond to  
- Identify which extensions are fully supported, partially supported, or unsupported  
- Identify which features within each extension are supported or unsupported  
- Produce a structured compatibility matrix and summary report  

---

## **Scope**

### **Included**
- Clone and analyze the StraightFour repository  
- Clone the OMI glTF Extensions repository **read‑only**  
- Parse entity definitions, schemas, and metadata from StraightFour  
- Parse extension specifications, schemas, and feature lists from OMI glTF Extensions  
- Build a mapping between StraightFour entities and glTF extensions  
- Identify missing or incomplete support  
- Generate a compatibility matrix  
- Generate a human‑readable summary report  
- Generate a JSON mapping file  
- Write all outputs **inside the StraightFour repo** under `docs/compatibility/`  

### **Excluded**
- No modifications to the OMI glTF Extensions repository  
- No PRs or commits to the OMI glTF Extensions repository  
- No implementation of missing features  
- No changes to StraightFour code  
- No UI or visualization work beyond markdown tables  

---

## **Architecture Notes**  
StraightFour entities typically include:

- Core entity types  
- Components  
- Behaviors  
- Metadata  
- Relationships  

OMI glTF extensions typically include:

- Extension name  
- Purpose  
- Schema additions  
- Required vs optional fields  
- Feature lists  
- Example usage  

The mapping should consider:

- Semantics  
- Data model alignment  
- Behavioral alignment  
- Required vs optional fields  
- Whether StraightFour implements equivalent functionality  

---

## **Data Model Requirements**

### Extract from StraightFour:
- Entity type name  
- Description  
- Fields / properties  
- Behaviors / capabilities  
- Required vs optional fields  
- Any metadata or annotations  

### Extract from OMI glTF Extensions:
- Extension name  
- Purpose  
- Schema fields  
- Required vs optional fields  
- Feature list  
- Example usage  
- Dependencies on other extensions  

---

## **Analysis Requirements**

### For each StraightFour entity:
- Identify matching glTF extension(s)  
- Determine if StraightFour fully implements the extension  
- Determine if StraightFour partially implements the extension  
- Determine if StraightFour does not implement the extension  
- Identify missing fields or behaviors  

### For each glTF extension:
- Determine if StraightFour supports it  
- Identify unsupported features  
- Identify partially supported features  
- Identify missing required fields  
- Identify missing optional fields  

---

## **Output Requirements**

### **1. Compatibility Matrix (Markdown Table)**  
Write to:

```
docs/compatibility/straightfour-gltf-matrix.md
```

Columns:

- Extension Name  
- StraightFour Entity  
- Support Level (Full / Partial / None)  
- Missing Features  
- Notes  

---

### **2. Detailed Report (Markdown)**  
Write to:

```
docs/compatibility/straightfour-gltf-report.md
```

Sections:

- Overview of StraightFour entity model  
- Overview of OMI glTF extension ecosystem  
- Mapping methodology  
- Per‑extension analysis  
- Per‑entity analysis  
- Summary of unsupported extensions  
- Summary of partially supported extensions  
- Recommendations for future support  

---

### **3. JSON Mapping File**  
Write to:

```
docs/compatibility/straightfour-gltf-mapping.json
```

Example structure:

```json
{
  "extensions": {
    "EXT_example": {
      "supported": true,
      "supported_features": ["featureA", "featureB"],
      "missing_features": ["featureC"],
      "mapped_entities": ["EntityType1"]
    }
  }
}
```

---

## **Constraints**
- Do not modify any code in StraightFour  
- Do not modify or write to the OMI glTF Extensions repository  
- Keep all analysis outputs inside StraightFour  
- Keep the diff small and safe  
- Do not generate placeholder content  
- Do not generate unrelated files  

---

## **Edge Cases**
- Extensions with no clear StraightFour equivalent  
- Entities that map to multiple extensions  
- Extensions that require other extensions  
- Extensions with optional vs required fields  
- Entities with incomplete documentation  

---

## **Test Requirements**
- Validate that all StraightFour entities were included  
- Validate that all OMI glTF extensions were included  
- Validate that the mapping is complete  
- Validate that the compatibility matrix is well‑formed  

---

## **Documentation Requirements**
- Add the compatibility matrix to `docs/compatibility/straightfour-gltf-matrix.md`  
- Add the detailed report to `docs/compatibility/straightfour-gltf-report.md`  
- Add the JSON mapping to `docs/compatibility/straightfour-gltf-mapping.json`  
