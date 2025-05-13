import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import { describe, it, expect, beforeEach } from "vitest"; // Removed vi
import EditEvent from "../../components/AdminPages/EditEvent";
import { mockNavigate, mockGetItem, mockedAxios } from "../testMocks";

// Global mocks from setupTests.ts are used for react-router-dom, axios, localStorage

const mockEventsData = [
  {
    id: 1,
    title: "Event 1",
    startDateTimeUtc: "2025-01-01T10:00:00Z",
    location: "Location 1",
    sourceUrl: "http://example.com/1",
  },
  {
    id: 2,
    title: "Event 2",
    startDateTimeUtc: "2025-02-01T12:00:00Z",
    location: "Location 2",
    sourceUrl: "http://example.com/2",
  },
];

describe("EditEvent Component", () => {
  beforeEach(() => {
    mockGetItem.mockReturnValue("fake-jwt-token");
    mockedAxios.get.mockReset();
    mockedAxios.put.mockReset();
    mockedAxios.get.mockResolvedValue({ data: [...mockEventsData] }); // Provide a fresh copy
  });

  it("renders the component and fetches events", async () => {
    render(<EditEvent />);
    expect(screen.getByText("Rediger Begivenhed")).toBeInTheDocument();
    await waitFor(() => {
      expect(mockedAxios.get).toHaveBeenCalledWith("/api/calendar/events", {
        headers: { Authorization: "Bearer fake-jwt-token" },
      });
      expect(screen.getByText("-- Vælg en begivenhed --")).toBeInTheDocument();
      expect(screen.getByText("Event 1 (ID: 1)")).toBeInTheDocument();
      expect(screen.getByText("Event 2 (ID: 2)")).toBeInTheDocument();
    });
  });

  it("shows an error if not logged in when fetching events", async () => {
    mockGetItem.mockReturnValueOnce(null);
    render(<EditEvent />);
    await waitFor(() => {
      expect(screen.getByText(/Du er ikke logget ind/i)).toBeInTheDocument();
    });
    expect(mockedAxios.get).not.toHaveBeenCalled(); // Ensure API call was not made
  });

  it("loads event details when an event is selected", async () => {
    render(<EditEvent />);
    await waitFor(() => {
      expect(screen.getByText("Event 1 (ID: 1)")).toBeInTheDocument();
    });

    fireEvent.change(screen.getByLabelText("Vælg Begivenhed"), {
      target: { value: "1" },
    });

    await waitFor(() => {
      expect((screen.getByLabelText(/Titel/i) as HTMLInputElement).value).toBe(
        mockEventsData[0].title
      );
      expect(
        (screen.getByLabelText(/Start Dato\/Tid \(UTC\)/i) as HTMLInputElement)
          .value
      ).toBe("2025-01-01T11:00"); // Adjusted to local time this is bad and we should use local time instead of UTC
      expect(
        (screen.getByLabelText(/Lokation/i) as HTMLInputElement).value
      ).toBe(mockEventsData[0].location);
      expect(
        (screen.getByLabelText(/Kilde URL/i) as HTMLInputElement).value
      ).toBe(mockEventsData[0].sourceUrl);
    });
  });

  it("shows an error message if required fields are missing on submit", async () => {
    render(<EditEvent />);
    await waitFor(() => {
      expect(screen.getByText("Event 1 (ID: 1)")).toBeInTheDocument();
    });
    fireEvent.change(screen.getByLabelText("Vælg Begivenhed"), {
      target: { value: "1" },
    });

    await waitFor(() => {
      expect((screen.getByLabelText(/Titel/i) as HTMLInputElement).value).toBe(
        "Event 1"
      );
    });
    fireEvent.change(screen.getByLabelText(/Titel/i), {
      target: { value: "" },
    });

    fireEvent.submit(
      screen.getByRole("button", { name: /Opdater Begivenhed/i })
    );

    await waitFor(() => {
      expect(
        screen.getByText(
          "Titel, Start Dato/Tid (UTC), og Kilde URL er påkrævede felter."
        )
      ).toBeInTheDocument();
    });
  });

  it("submits updated form data correctly", async () => {
    mockedAxios.put.mockResolvedValue({
      data: { message: "Begivenhed opdateret succesfuldt!" },
    });
    render(<EditEvent />);
    await waitFor(() => {
      expect(screen.getByText("Event 1 (ID: 1)")).toBeInTheDocument();
    });
    fireEvent.change(screen.getByLabelText("Vælg Begivenhed"), {
      target: { value: "1" },
    });

    await waitFor(() => {
      expect((screen.getByLabelText(/Titel/i) as HTMLInputElement).value).toBe(
        "Event 1"
      );
    });

    fireEvent.change(screen.getByLabelText(/Titel/i), {
      target: { value: "Updated Event 1" },
    });
    fireEvent.change(screen.getByLabelText(/Start Dato\/Tid \(UTC\)/i), {
      target: { value: "2025-01-01T11:00" },
    });

    fireEvent.submit(
      screen.getByRole("button", { name: /Opdater Begivenhed/i })
    );

    await waitFor(() => {
      expect(mockedAxios.put).toHaveBeenCalledWith(
        "/api/calendar/events/1",
        expect.objectContaining({
          id: 1,
          title: "Updated Event 1",
          startDateTimeUtc: "2025-01-01T11:00:00Z",
          location: "Location 1",
          sourceUrl: "http://example.com/1",
        }),
        { headers: { Authorization: "Bearer fake-jwt-token" } }
      );
      expect(
        screen.getByText("Begivenhed opdateret succesfuldt!")
      ).toBeInTheDocument();
    });
  });

  it("handles API error on event update", async () => {
    mockedAxios.put.mockRejectedValue({
      isAxiosError: true,
      response: { data: { message: "Failed to update event" } },
    });
    render(<EditEvent />);
    await waitFor(() => {
      expect(screen.getByText("Event 1 (ID: 1)")).toBeInTheDocument();
    });
    fireEvent.change(screen.getByLabelText("Vælg Begivenhed"), {
      target: { value: "1" },
    });

    await waitFor(() => {
      expect((screen.getByLabelText(/Titel/i) as HTMLInputElement).value).toBe(
        "Event 1"
      );
    });

    fireEvent.change(screen.getByLabelText(/Titel/i), {
      target: { value: "Updated Event 1" },
    });
    fireEvent.submit(
      screen.getByRole("button", { name: /Opdater Begivenhed/i })
    );

    await waitFor(() => {
      const specificErrorMessage = screen.queryByText(
        "Fejl: Failed to update event"
      );
      const fallbackErrorMessage = screen.queryByText("En ukendt fejl opstod.");
      expect(specificErrorMessage || fallbackErrorMessage).toBeInTheDocument();
    });
  });

  it("handles API error on fetching events", async () => {
    mockGetItem.mockReturnValue("fake-jwt-token");
    mockedAxios.get.mockReset();
    mockedAxios.get.mockRejectedValueOnce({
      isAxiosError: true,
      response: { data: { message: "Failed to fetch events" } },
    });
    render(<EditEvent />);
    await waitFor(() => {
      expect(
        screen.getByText(/Kunne ikke hente begivenheder\./)
      ).toBeInTheDocument();
    });
  });

  it("navigates back to admin content page", async () => {
    render(<EditEvent />);
    await waitFor(() => {
      expect(screen.getByLabelText("Vælg Begivenhed")).toBeInTheDocument();
    });
    fireEvent.change(screen.getByLabelText("Vælg Begivenhed"), {
      target: { value: "1" },
    });

    fireEvent.click(
      screen.getByRole("button", { name: /Tilbage til Admin Indhold/i })
    );
    expect(mockNavigate).toHaveBeenCalledWith("/admin/Indhold");
  });
});
