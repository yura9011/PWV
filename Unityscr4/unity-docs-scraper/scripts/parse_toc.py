import requests
import json

def download_and_process_toc():
    url = "https://docs.unity3d.com/ScriptReference/docdata/toc.js"
    try:
        print("Downloading TOC from:", url)
        response = requests.get(url)
        response.raise_for_status()
        content = response.text
        print(f"Downloaded {len(content)} characters")

        if content.startswith("var toc = "):
            json_str = content[10:]  # strip 'var toc = '
            print("Stripped 'var toc =' prefix")
        else:
            json_str = content
            print("No 'var toc =' prefix found, using full content")

        toc = json.loads(json_str)
        print("Successfully parsed JSON")
    except requests.RequestException as e:
        print(f"Network error: {e}")
        return
    except json.JSONDecodeError as e:
        print(f"JSON parsing error: {e}")
        return

    all_items = []

    def traverse(node, depth=0, path=""):
        indent = "  " * depth
        if 'title' in node:
            current_title = node['title']
            current_path = f"{path}.{current_title}" if path else current_title
            print(f"{indent}Processing node: {current_title} (path: {current_path})")

            # Add this item to our collection if it's not a category header
            if current_title not in ['Classes', 'Structs', 'Enums', 'Interfaces', 'Namespaces', 'Variables', 'Functions', 'Properties', 'Events']:
                all_items.append(current_title)
        else:
            current_path = path

        # Process children
        if 'children' in node:
            for child in node['children']:
                traverse(child, depth + 1, current_path)

    if 'children' in toc and toc['children']:
        print("Starting traversal of first child...")
        traverse(toc['children'][0])  # UnityEngine
    else:
        print("No children found in TOC")

    # Remove duplicates while preserving order
    unique_items = []
    seen = set()
    for item in all_items:
        if item and item not in seen:
            unique_items.append(item)
            seen.add(item)

    print(f"Found {len(unique_items)} total items")

    with open('../unity_engine_all_items.txt', 'w', encoding='utf-8') as f:
        for item in sorted(unique_items):
            f.write(item + '\n')
    print("All items saved to ../unity_engine_all_items.txt")

if __name__ == "__main__":
    download_and_process_toc()
