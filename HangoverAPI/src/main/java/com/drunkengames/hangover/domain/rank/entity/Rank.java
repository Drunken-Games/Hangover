package com.drunkengames.hangover.domain.rank.entity;

import jakarta.persistence.*;
import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Getter;
import lombok.NoArgsConstructor;

@AllArgsConstructor
@NoArgsConstructor
@Getter
@Builder
@Entity
@Table(name = "T_Rank")
public class Rank {
    @Column(name = "rank_id")
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;

    @Column(name = "user_nickname", length = 100, nullable = false)
    private String userNickname;

    @Column(name = "final_money")
	private Integer finalMoney;

    @Column(name = "final_day")
	private Integer finalDay;

}
