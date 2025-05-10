import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach, Mock } from "vitest";
import { BrowserRouter } from "react-router-dom";
import EditPoll from "../components/AdminPages/EditPoll";
import { mockNavigate, mockedAxios, mockGetItem } from "./testMocks";

const mockPollsSummaryList = [
  { id: 1, question: "Poll One Question", politicianTwitterId: "twit1" },
  { id: 2, question: "Poll Two Question", politicianTwitterId: "twit2" },
];

const mockPoliticiansList = [
  { id: "aktor1", navn: "Politician Uno", politicianTwitterId: 111 },
  { id: "aktor2", navn: "Politician Dos", politicianTwitterId: 222 },
];

const mockSelectedPollDetails: any = {
  id: 1,
  question: "Poll One Question Detailed",
  options: [
    { id: 101, optionText: "Opt A" },
    { id: 102, optionText: "Opt B" },
  ],
  endedAt: "2024-01-01T00:00:00Z",
  politicianId: "twitterIdForAktor1",
};

describe("EditPoll", () => {
  beforeEach(() => {
    mockGetItem.mockReturnValue("fake-jwt-token");
    (mockedAxios.get as Mock).mockImplementation((url: string) => {
      if (url === "/api/polls") {
        return Promise.resolve({ data: mockPollsSummaryList });
      }
      if (url === "/api/aktor/all") {
        return Promise.resolve({ data: mockPoliticiansList });
      }
      if (url === `/api/polls/${mockSelectedPollDetails.id}`) {
        return Promise.resolve({ data: mockSelectedPollDetails });
      }
      if (url.startsWith("/api/subscription/lookup/politicianTwitterId")) {
        const aktorId = url.split("=")[1];
        const politician = mockPoliticiansList.find((p) => p.id === aktorId);
        return Promise.resolve({ data: { politicianTwitterId: politician?.politicianTwitterId || 789 } });
      }
      if (url.startsWith("/api/administrator/lookup/aktorId")) {
        return Promise.resolve({ data: { aktorId: "aktor1" } });
      }
      return Promise.reject(new Error(`Unknown GET URL: ${url}`));
    });
    mockedAxios.put.mockResolvedValue({ data: {} });
  });

  it("renders, fetches polls and politicians", async () => {
    render(
      <BrowserRouter>
        <EditPoll />
      </BrowserRouter>
    );
    expect(screen.getByText("Rediger Poll")).toBeInTheDocument();
    await waitFor(() => {
      expect(mockedAxios.get).toHaveBeenCalledWith("/api/polls", expect.anything());
      expect(mockedAxios.get).toHaveBeenCalledWith("/api/aktor/all", expect.anything());
    });
    mockPollsSummaryList.forEach((p) => expect(screen.getByText(p.question)).toBeInTheDocument());
    expect(screen.queryByLabelText("Vælg Politiker")).not.toBeInTheDocument();
  });

  it("fetches and displays poll details when a poll is selected", async () => {
    render(
      <BrowserRouter>
        <EditPoll />
      </BrowserRouter>
    );
    await waitFor(() => expect(screen.getByText(mockPollsSummaryList[0].question)).toBeInTheDocument());

    const pollSelect = screen.getByLabelText("Vælg Poll") as HTMLSelectElement;
    fireEvent.change(pollSelect, { target: { value: String(mockSelectedPollDetails.id) } });

    await waitFor(() => {
      expect(mockedAxios.get).toHaveBeenCalledWith(`/api/polls/${mockSelectedPollDetails.id}`, expect.anything());
      expect(mockedAxios.get).toHaveBeenCalledWith(
        `/api/administrator/lookup/aktorId?twitterId=${mockSelectedPollDetails.politicianId}`,
        expect.anything()
      );
    });

    await waitFor(() => {
      expect(screen.getByLabelText("Vælg Politiker")).toBeInTheDocument();
      expect(screen.getByDisplayValue(mockPoliticiansList[0].navn)).toBeInTheDocument();
    });

    expect(screen.getByPlaceholderText("Skriv spørgsmål 1 her...")).toHaveValue(mockSelectedPollDetails.question);
    expect(screen.getByPlaceholderText("Svarmulighed 1.1")).toHaveValue(mockSelectedPollDetails.options[0].optionText);
    expect(screen.getByLabelText("Slutdato (valgfri)")).toHaveValue("2024-01-01");
  });

  it("submits updated form data", async () => {
    render(
      <BrowserRouter>
        <EditPoll />
      </BrowserRouter>
    );
    await waitFor(() => expect(screen.getByText(mockPollsSummaryList[0].question)).toBeInTheDocument());

    fireEvent.change(screen.getByLabelText("Vælg Poll"), { target: { value: String(mockSelectedPollDetails.id) } });

    await waitFor(() => {
      expect(screen.getByPlaceholderText("Skriv spørgsmål 1 her...")).toHaveValue(mockSelectedPollDetails.question);
    });

    const politicianSelect = screen.getByLabelText("Vælg Politiker") as HTMLSelectElement;
    fireEvent.change(politicianSelect, { target: { value: mockPoliticiansList[1].id } });

    await waitFor(() => {
      expect(mockedAxios.get).toHaveBeenCalledWith(
        `/api/subscription/lookup/politicianTwitterId?aktorId=${mockPoliticiansList[1].id}`,
        expect.anything()
      );
    });

    fireEvent.change(screen.getByPlaceholderText("Skriv spørgsmål 1 her..."), { target: { value: "Updated Question?" } });
    fireEvent.change(screen.getByPlaceholderText("Svarmulighed 1.1"), { target: { value: "Updated Opt A" } });

    fireEvent.click(screen.getByText("Opdater Poll"));

    await waitFor(() => {
      expect(mockedAxios.put).toHaveBeenCalledWith(
        `/api/polls/${mockSelectedPollDetails.id}`,
        {
          question: "Updated Question?",
          options: ["Updated Opt A", mockSelectedPollDetails.options[1].optionText],
          politicianTwitterId: 3,
          endedAt: new Date("2024-01-01T00:00:00Z").toISOString(),
        },
        expect.anything()
      );
    });
    expect(mockNavigate).toHaveBeenCalledWith("/admin/polls");
  });

  it("shows alert if politician is not selected on submit", async () => {
    // Test-specific mock implementation for axios.get
    (mockedAxios.get as Mock).mockImplementation((url: string) => {
      if (url === "/api/polls") {
        return Promise.resolve({ data: mockPollsSummaryList });
      }
      if (url === "/api/aktor/all") {
        return Promise.resolve({ data: mockPoliticiansList });
      }
      if (url === `/api/polls/${mockSelectedPollDetails.id}`) {
        // Crucial for this test: return poll details with politicianId as null
        return Promise.resolve({ data: { ...mockSelectedPollDetails, politicianId: null } });
      }
      // Provide default successful responses for other potential GET calls to avoid unhandled rejections
      if (url.startsWith("/api/subscription/lookup/politicianTwitterId")) {
        return Promise.resolve({ data: { politicianTwitterId: 12345 } }); // Default mock value
      }
      if (url.startsWith("/api/administrator/lookup/aktorId")) {
        return Promise.resolve({ data: { aktorId: "aktor1" } }); // Default mock value
      }
      return Promise.reject(new Error(`Unhandled GET URL in test-specific mock: ${url}`));
    });

    render(
      <BrowserRouter>
        <EditPoll />
      </BrowserRouter>
    );

    // Wait for initial data to load (polls list)
    await waitFor(() => expect(screen.getByText(mockPollsSummaryList[0].question)).toBeInTheDocument());

    // Select the poll that will have null politicianId
    fireEvent.change(screen.getByLabelText("Vælg Poll"), { target: { value: String(mockSelectedPollDetails.id) } });

    // Wait for the form to populate and verify politician selection is empty
    await waitFor(() => {
      expect(screen.getByPlaceholderText("Skriv spørgsmål 1 her...")).toBeInTheDocument();
      const politicianSelect = screen.getByLabelText("Vælg Politiker") as HTMLSelectElement;
      expect(politicianSelect).toBeInTheDocument();
      expect(politicianSelect.value).toBe(""); // Ensure no politician is selected
    });

    // Attempt to submit the form
    fireEvent.click(screen.getByText("Opdater Poll"));

    // Check that the alert was called
    await waitFor(() => {
      expect(window.alert).toHaveBeenCalledWith("Du skal vælge en politiker.");
    });
    // Ensure navigation did not occur
    expect(mockNavigate).not.toHaveBeenCalled();
  });
});
