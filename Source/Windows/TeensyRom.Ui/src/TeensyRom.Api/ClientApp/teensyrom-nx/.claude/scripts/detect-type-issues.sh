#!/bin/bash

# TypeScript Error Detection Script
# Generic detection of type assignment issues without hardcoding specific types

set -euo pipefail

# Default to checking all files if no argument provided
TARGET_FILE="${1:-}"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to extract file path from TypeScript error
extract_file_path() {
    local error_line="$1"
    # TypeScript errors have format: file(line,column): error TS####: message
    echo "$error_line" | sed -n 's/^\([^:(]*\)(.*/\1/p'
}

# Function to extract line number from TypeScript error
extract_line_number() {
    local error_line="$1"
    echo "$error_line" | sed -n 's/^[^:(]*(\([^,]*\),.*/\1/p'
}

# Function to extract the column number from TypeScript error
extract_column_number() {
    local error_line="$1"
    echo "$error_line" | sed -n 's/^[^:(]*([^,]*,\([^)]*\)).*/\1/p'
}

# Function to check if error line matches our pattern
is_type_assignment_error() {
    local error_line="$1"
    # Check for "Type 'X' is not assignable to type 'Y'" pattern
    echo "$error_line" | grep -q "Type.*is not assignable to type"
}

# Function to extract the "from" type from a type assignment error
extract_actual_type() {
    local error_message="$1"
    # Pattern: Type 'X' is not assignable to type 'Y'
    # Clean up quotes, backslashes, and trailing punctuation, then extract
    echo "$error_message" | sed "s/''/'/g" | sed "s/'//g" | tr -d "\\" | awk -F "Type | is not assignable to type " '{print $2}' | awk '{print $1}' | sed 's/\.$//'
}

# Function to extract the "to" type from a type assignment error
extract_declared_type() {
    local error_message="$1"
    # Pattern: Type 'X' is not assignable to type 'Y'
    # Clean up quotes, backslashes, and trailing punctuation, then extract
    echo "$error_message" | sed "s/''/'/g" | sed "s/'//g" | tr -d "\\" | awk -F " is not assignable to type " '{print $2}' | awk '{print $1}' | sed 's/\.$//'
}

# Function to check if a type assignment error represents type information loss
is_type_information_loss() {
    local actual_type="$1"
    local declared_type="$2"

    # Check if we're losing type information (actual type is more specific than declared)
    case "$declared_type" in
        "unknown"|"any"|"object")
            return 0
            ;;
        *)
            # Check if declared type is more generic than actual type
            # Example: actual_type="string", declared_type="unknown" -> this is type loss
            if [[ "$actual_type" != "unknown" && "$actual_type" != "any" && "$declared_type" == "unknown" ]]; then
                return 0
            fi

            # Check if actual type is a generic type that should be preserved
            if [[ "$actual_type" == *"<"*">"* ]] && [[ "$declared_type" == *"unknown"* ]]; then
                return 0
            fi
            return 1
            ;;
    esac
}

# Function to generate a suggested fix
generate_suggestion() {
    local actual_type="$1"
    local declared_type="$2"
    local file_path="$3"
    local line_number="$4"

    if is_type_information_loss "$actual_type" "$declared_type"; then
        echo "Change return type from '$declared_type' to '$actual_type'"
    else
        echo "Review type compatibility: $actual_type → $declared_type"
    fi
}

# Function to analyze a single TypeScript error
analyze_error() {
    local error_line="$1"

    # Skip if not a type-related error
    if ! is_type_assignment_error "$error_line"; then
        return
    fi

    local file_path
    local line_number
    local column_number
    local error_message
    local actual_type
    local declared_type

    file_path=$(extract_file_path "$error_line")
    line_number=$(extract_line_number "$error_line")
    column_number=$(extract_column_number "$error_line")

    # Extract error message after the error code
    error_message=$(echo "$error_line" | sed 's/^[^:]*: error [^:]*: //')

    # Extract types from the error message
    actual_type=$(extract_actual_type "$error_message")
    declared_type=$(extract_declared_type "$error_message")

    # Skip if we couldn't extract both types
    if [[ -z "$actual_type" || -z "$declared_type" ]]; then
        return
    fi

    # If targeting a specific file, skip others
    if [[ -n "$TARGET_FILE" && "$file_path" != *"$TARGET_FILE"* ]]; then
        return
    fi

    # Check if this represents type information loss
    if is_type_information_loss "$actual_type" "$declared_type"; then
        echo -e "${RED}Type Information Loss Detected:${NC}"
        echo -e "  ${BLUE}File:${NC} $file_path:$line_number:$column_number"
        echo -e "  ${BLUE}Issue:${NC} Type '$actual_type' is not assignable to type '$declared_type'"
        echo -e "  ${GREEN}Suggestion:${NC} $(generate_suggestion "$actual_type" "$declared_type" "$file_path" "$line_number")"
        echo
    fi
}

# Main function
main() {
    echo -e "${BLUE}TypeScript Type Issue Detection${NC}"
    echo "=================================="
    echo

    # Build tsc command
    local tsc_cmd="npx tsc --noEmit --pretty false"
    if [[ -n "$TARGET_FILE" ]]; then
        tsc_cmd="$tsc_cmd $TARGET_FILE"
    fi

    # Get TypeScript errors
    local ts_errors
    if ! ts_errors=$($tsc_cmd 2>&1); then
        echo -e "${YELLOW}TypeScript errors found. Analyzing type issues...${NC}"
        echo

        # Process each error line
        echo "$ts_errors" | while IFS= read -r line; do
            analyze_error "$line"
        done

        echo -e "${BLUE}Analysis complete.${NC}"
    else
        echo -e "${GREEN}No TypeScript errors found.${NC}"
    fi
}

# Check if running in the right directory
if [[ ! -f "package.json" ]]; then
    echo "Error: Must run from project root (package.json not found)"
    exit 1
fi

# Run main function
main "$@"