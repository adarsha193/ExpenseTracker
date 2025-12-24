#!/bin/bash
# Test Execution Script for ExpenseTracker Unit Tests
# Usage: bash run_tests.sh [option]

set -e

PROJECT_ROOT="/Users/adarshahebbar/Documents/Maui.net/ExpenseTracker"
TEST_PROJECT="$PROJECT_ROOT/Tests"

echo "=========================================="
echo "ExpenseTracker Unit Test Suite"
echo "=========================================="
echo ""

# Color codes
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Parse command line arguments
COMMAND=${1:-"all"}

case $COMMAND in
    "all"|"")
        echo -e "${BLUE}Running all tests...${NC}"
        cd "$PROJECT_ROOT"
        dotnet test Tests/ --logger "console;verbosity=detailed"
        ;;
    
    "quick")
        echo -e "${BLUE}Running quick test suite (without output)...${NC}"
        cd "$PROJECT_ROOT"
        dotnet test Tests/ --logger "minimal"
        ;;
    
    "firebase")
        echo -e "${BLUE}Running Firebase Service tests...${NC}"
        cd "$PROJECT_ROOT"
        dotnet test Tests/ --filter "FirebaseRealtimeDbServiceTests" --logger "console;verbosity=detailed"
        ;;
    
    "budget")
        echo -e "${BLUE}Running Budget Alert tests...${NC}"
        cd "$PROJECT_ROOT"
        dotnet test Tests/ --filter "BudgetAlertServiceTests" --logger "console;verbosity=detailed"
        ;;
    
    "ai")
        echo -e "${BLUE}Running AI Suggestions tests...${NC}"
        cd "$PROJECT_ROOT"
        dotnet test Tests/ --filter "AISuggestionsServiceTests" --logger "console;verbosity=detailed"
        ;;
    
    "notification")
        echo -e "${BLUE}Running Notification Service tests...${NC}"
        cd "$PROJECT_ROOT"
        dotnet test Tests/ --filter "NotificationServiceTests" --logger "console;verbosity=detailed"
        ;;
    
    "models")
        echo -e "${BLUE}Running Model tests...${NC}"
        cd "$PROJECT_ROOT"
        dotnet test Tests/ --filter "ModelTests" --logger "console;verbosity=detailed"
        ;;
    
    "viewmodel")
        echo -e "${BLUE}Running Dashboard ViewModel tests...${NC}"
        cd "$PROJECT_ROOT"
        dotnet test Tests/ --filter "DashboardPageViewModelTests" --logger "console;verbosity=detailed"
        ;;
    
    "coverage")
        echo -e "${BLUE}Running tests with coverage analysis...${NC}"
        cd "$PROJECT_ROOT"
        dotnet test Tests/ /p:CollectCoverage=true /p:CoverageFormat=lcov /p:CoverageFileName=coverage.lcov
        echo -e "${GREEN}Coverage report generated: coverage.lcov${NC}"
        ;;
    
    "watch")
        echo -e "${BLUE}Running tests in watch mode (re-runs on file changes)...${NC}"
        cd "$PROJECT_ROOT"
        dotnet watch test Tests/
        ;;
    
    "list")
        echo -e "${BLUE}Listing all available tests...${NC}"
        cd "$PROJECT_ROOT"
        dotnet test Tests/ --list-tests
        ;;
    
    "build")
        echo -e "${BLUE}Building test project...${NC}"
        cd "$PROJECT_ROOT"
        dotnet build Tests/ExpenseTracker.Tests.csproj
        ;;
    
    "clean")
        echo -e "${BLUE}Cleaning test project...${NC}"
        cd "$PROJECT_ROOT"
        dotnet clean Tests/
        ;;
    
    "help"|"-h"|"--help")
        cat << EOF
${GREEN}ExpenseTracker Test Execution Script${NC}

Usage: bash run_tests.sh [command]

Commands:
    all            Run all tests (default)
    quick          Run tests with minimal output
    firebase       Run Firebase Service tests only
    budget         Run Budget Alert tests only
    ai             Run AI Suggestions tests only
    notification   Run Notification Service tests only
    models         Run Model tests only
    viewmodel      Run ViewModel tests only
    coverage       Run tests with coverage analysis
    watch          Run tests in watch mode (re-runs on changes)
    list           List all available tests
    build          Build test project
    clean          Clean test project
    help           Show this help message

Examples:
    bash run_tests.sh                 # Run all tests
    bash run_tests.sh firebase        # Run Firebase tests
    bash run_tests.sh coverage        # Generate coverage report
    bash run_tests.sh watch           # Watch mode for development

${YELLOW}Quick Commands:${NC}
    cd /Users/adarshahebbar/Documents/Maui.net/ExpenseTracker
    dotnet test Tests/                          # All tests
    dotnet test Tests/ --filter "BudgetAlertServiceTests"  # Specific service
    dotnet test Tests/ --list-tests             # List tests

EOF
        ;;
    
    *)
        echo -e "${YELLOW}Unknown command: $COMMAND${NC}"
        echo "Use 'bash run_tests.sh help' for available commands"
        exit 1
        ;;
esac

echo ""
echo -e "${GREEN}✓ Test execution completed${NC}"
echo "=========================================="
