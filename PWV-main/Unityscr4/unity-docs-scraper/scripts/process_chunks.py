import json
import os
from typing import Dict, List
import re

def clean_text(text: str) -> str:
    """
    Clean text by removing extra whitespace and formatting issues
    
    Args:
        text: Input text to clean
        
    Returns:
        Cleaned text
    """
    if not text:
        return ""
    
    # Replace multiple whitespace with single space
    cleaned = re.sub(r'\s+', ' ', text)
    # Remove leading/trailing whitespace
    cleaned = cleaned.strip()
    return cleaned

def convert_to_mcp_format(class_data: Dict) -> str:
    """
    Convert a single class's data to MCP-indexable format
    
    Args:
        class_data: Dictionary containing class information
        
    Returns:
        String in MCP-indexable format
    """
    class_name = class_data.get('class_name', 'Unknown')
    description = clean_text(class_data.get('description', ''))
    properties = class_data.get('properties', [])
    methods = class_data.get('methods', [])
    constructors = class_data.get('constructors', [])
    examples = class_data.get('examples', [])
    url = class_data.get('url', '')
    
    lines = []
    
    # Add class header
    lines.append(f"// Unity Class: {class_name}")
    if url:
        lines.append(f"// Documentation: {url}")
    lines.append("")
    
    # Add description
    if description:
        lines.append(f"// Description: {description}")
        lines.append("")
    
    # Add properties section
    if properties:
        lines.append("// Properties:")
        for prop in properties:
            prop_name = clean_text(prop.get('name', ''))
            prop_desc = clean_text(prop.get('description', ''))
            if prop_name:
                lines.append(f"// - {prop_name}: {prop_desc}")
        lines.append("")
    
    # Add constructors section
    if constructors:
        lines.append("// Constructors:")
        for constructor in constructors:
            ctor_name = clean_text(constructor.get('name', ''))
            ctor_desc = clean_text(constructor.get('description', ''))
            if ctor_name:
                lines.append(f"// - {ctor_name}(): {ctor_desc}")
        lines.append("")
    
    # Add methods section
    if methods:
        lines.append("// Methods:")
        for method in methods:
            method_name = clean_text(method.get('name', ''))
            method_desc = clean_text(method.get('description', ''))
            if method_name:
                lines.append(f"// - {method_name}(): {method_desc}")
        lines.append("")
    
    # Add examples section
    if examples:
        lines.append("// Code Examples:")
        for i, example in enumerate(examples, 1):
            lines.append(f"// Example {i}:")
            # Format the example code nicely
            example_lines = example.split('\n')
            for ex_line in example_lines:
                lines.append(f"//   {ex_line}")
            lines.append("")
    
    # Add a pseudo-code representation for better indexing
    lines.append(f"// Pseudo-code representation of {class_name}")
    lines.append(f"public class {class_name} {{")
    
    # Add property stubs
    for prop in properties:
        prop_name = clean_text(prop.get('name', ''))
        if prop_name:
            lines.append(f"    public var {prop_name}; // {clean_text(prop.get('description', ''))}")
    
    # Add method stubs
    for method in methods:
        method_name = clean_text(method.get('name', ''))
        if method_name:
            lines.append(f"    public void {method_name}(); // {clean_text(method.get('description', ''))}")
    
    lines.append("}")
    
    return '\n'.join(lines)

def process_json_batches(input_dir: str = "data", output_dir: str = "index") -> None:
    """
    Process all JSON batch files and convert them to MCP-indexable text files
    
    Args:
        input_dir: Directory containing JSON batch files
        output_dir: Directory to output text files for MCP indexing
    """
    os.makedirs(output_dir, exist_ok=True)
    
    # Find all batch JSON files
    batch_files = []
    for filename in os.listdir(input_dir):
        if filename.startswith("batch_") and filename.endswith(".json"):
            batch_files.append(os.path.join(input_dir, filename))
    
    if not batch_files:
        print(f"No batch files found in {input_dir}")
        # Create a sample batch file for testing
        create_sample_batch(input_dir)
        batch_files = [os.path.join(input_dir, "batch_001.json")]
    
    print(f"Processing {len(batch_files)} batch files...")
    
    total_processed = 0
    
    for batch_file in batch_files:
        print(f"Processing batch: {batch_file}")
        
        try:
            with open(batch_file, 'r', encoding='utf-8') as f:
                batch_data = json.load(f)
        except FileNotFoundError:
            print(f"Batch file not found: {batch_file}")
            continue
        except json.JSONDecodeError:
            print(f"Invalid JSON in file: {batch_file}")
            continue
        
        print(f"  Found {len(batch_data)} classes in batch")
        
        for class_data in batch_data:
            class_name = class_data.get('class_name', '').strip()
            
            if not class_name:
                print("  Skipping entry with no class name")
                continue
            
            # Create output filename based on class name
            safe_filename = "".join(c for c in class_name if c.isalnum() or c in (' ', '-', '_')).rstrip()
            output_file = os.path.join(output_dir, f"{safe_filename}.txt")
            
            # Convert to MCP format and write to file
            mcp_content = convert_to_mcp_format(class_data)
            
            with open(output_file, 'w', encoding='utf-8') as f:
                f.write(mcp_content)
            
            total_processed += 1
            print(f"  Created: {output_file}")
    
    print(f"Processed {total_processed} classes in total")

def create_sample_batch(input_dir: str):
    """
    Create a sample batch file for testing purposes
    
    Args:
        input_dir: Directory to create the sample batch file
    """
    os.makedirs(input_dir, exist_ok=True)
    
    # Sample data for testing
    sample_data = [
        {
            "class_name": "GameObject",
            "description": "Base class for all entities in Unity scenes.",
            "properties": [
                {
                    "name": "transform",
                    "description": "The Transform attached to this GameObject."
                },
                {
                    "name": "activeSelf", 
                    "description": "The local active state of this GameObject."
                }
            ],
            "methods": [
                {
                    "name": "GetComponent",
                    "description": "Returns the component of Type type if the game object has one attached, null if it doesn't."
                },
                {
                    "name": "SetActive", 
                    "description": "Activates/Deactivates the GameObject."
                }
            ],
            "constructors": [],
            "examples": [
                "using UnityEngine;\n\npublic class Example : MonoBehaviour {\n    void Start() {\n        GameObject obj = new GameObject();\n        obj.name = \"Example Object\";\n    }\n}"
            ],
            "url": "https://docs.unity3d.com/ScriptReference/GameObject.html"
        },
        {
            "class_name": "Transform",
            "description": "Position, rotation and scale of an object.",
            "properties": [
                {
                    "name": "position",
                    "description": "The position of the transform in world space."
                },
                {
                    "name": "rotation",
                    "description": "The rotation of the transform in world space."
                },
                {
                    "name": "localScale", 
                    "description": "The scale of the transform relative to the parent."
                }
            ],
            "methods": [
                {
                    "name": "Translate",
                    "description": "Moves the transform in the direction and distance of translation."
                },
                {
                    "name": "Rotate",
                    "description": "Applies a rotation of eulerAngles degrees around the transform's local z, x and y axis."
                }
            ],
            "constructors": [],
            "examples": [
                "using UnityEngine;\n\npublic class Example : MonoBehaviour {\n    void Update() {\n        transform.Translate(Vector3.forward * Time.deltaTime);\n    }\n}"
            ],
            "url": "https://docs.unity3d.com/ScriptReference/Transform.html"
        }
    ]
    
    sample_file = os.path.join(input_dir, "batch_001.json")
    with open(sample_file, 'w', encoding='utf-8') as f:
        json.dump(sample_data, f, indent=2, ensure_ascii=False)
    
    print(f"Created sample batch file: {sample_file}")

def main():
    print("Starting process to convert JSON batches to MCP-indexable text files...")
    process_json_batches()
    print("Conversion complete!")

if __name__ == "__main__":
    main()